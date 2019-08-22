using System;
using System.Buffers;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace AuthorizedBuyersHelpers {

    /// <summary>
    /// Authorized Buyers で使用される暗号化スキームを扱うクラス。
    /// <para>
    /// 暗号文の構造は以下の通り:
    /// IV(16 bytes) || Payload(max 15380 bytes) || Signature(4 bytes)
    /// </para>
    /// 平文は XOR 暗号により暗号化される為、ペイロードと平文の長さは同じです。
    /// </summary>
    /// <remarks>
    /// https://github.com/google/openrtb-doubleclick/wiki#cryptography
    /// https://developers.google.com/authorized-buyers/rtb/response-guide/decrypt-price
    /// </remarks>
    public class ABCrypto {

        /// <summary>
        /// 暗号化に使用される初期化ベクトルのバイトサイズ。
        /// </summary>
        public const int IVSize = 16;

        /// <summary>
        /// 暗号文に含まれる署名のバイトサイズ。
        /// </summary>
        public const int SignatureSize = 4;

        /// <summary>
        /// 暗号文からペイロードを除いたバイトサイズ。<see cref="IVSize"/> + <see cref="SignatureSize"/>。
        /// </summary>
        public const int OverheadSize = IVSize + SignatureSize;

        /// <summary>
        /// ペイロードの最大バイトサイズ。
        /// </summary>
        public const int MaxPayloadSize = _sectionSize * _maxSections;

        /// <summary>
        /// ペイロードを構成するセクションのバイトサイズ。
        /// </summary>
        private const int _sectionSize = 20;

        /// <summary>
        /// ペイロードを構成するセクションの最大個数。
        /// </summary>
        private const int _maxSections = 3 * 256 + 1;

        private readonly IABCryptoKeys _keys;

        /// <summary>
        /// <see cref="ABCrypto"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="keys">Authorized Buyers の暗号化スキームが必要とするキーセット。</param>
        /// <exception cref="ArgumentNullException"><paramref name="keys"/> is <c>null</c>.</exception>
        public ABCrypto(IABCryptoKeys keys) {
            _keys = keys ?? throw new ArgumentNullException(nameof(keys));
        }

        /// <summary>
        /// 平文を暗号化します。
        /// </summary>
        /// <param name="plainBytes">暗号化対象の平文。</param>
        /// <param name="inputIV">
        /// 暗号化に使用される初期化ベクトル。
        /// 長さが <see cref="IVSize"/> に満たない場合は不足分を 0 で埋め、<see cref="IVSize"/> を超える場合は <see cref="IVSize"/> まで切り詰めて使用します。
        /// </param>
        /// <param name="destination">
        /// 暗号文の書き込み先となる <see cref="byte"/> スパン。
        /// 少なくとも <paramref name="plainBytes"/> の長さと <see cref="OverheadSize"/> を加算した値以上の長さが必要です。
        /// </param>
        /// <param name="bytesWritten">
        /// 実際に <paramref name="destination"/> に書き込まれたバイトサイズ。
        /// <paramref name="plainBytes"/> の長さと <see cref="OverheadSize"/> を加算した値になります。
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="plainBytes"/> の長さが <see cref="MaxPayloadSize"/> を超えています。
        /// または、<paramref name="destination"/> の長さが <paramref name="plainBytes"/> の長さと <see cref="OverheadSize"/> を加算した値を満たしません。
        /// </exception>
        public void Encrypt(ReadOnlySpan<byte> plainBytes, ReadOnlySpan<byte> inputIV, Span<byte> destination, out int bytesWritten) {
            if (plainBytes.Length > MaxPayloadSize) { throw new ArgumentOutOfRangeException($"{nameof(plainBytes)} の長さが {plainBytes.Length} です。これは最大長 {MaxPayloadSize} を超えています。", nameof(plainBytes)); }
            if (destination.Length < plainBytes.Length + OverheadSize) { throw new ArgumentOutOfRangeException($"{nameof(destination)} の長さは {destination.Length} ですが、少なくとも {plainBytes.Length + OverheadSize} 必要です。", nameof(destination)); }

            var success = TryEncrypt(plainBytes, inputIV, destination, out bytesWritten);
            Debug.Assert(success);
        }

        /// <summary>
        /// 平文を暗号化します。
        /// </summary>
        /// <param name="plainBytes">暗号化対象の平文。</param>
        /// <param name="inputIV">
        /// 暗号化に使用される初期化ベクトル。
        /// 長さが <see cref="IVSize"/> に満たない場合は不足分を 0 で埋め、<see cref="IVSize"/> を超える場合は <see cref="IVSize"/> まで切り詰めて使用します。
        /// </param>
        /// <param name="destination">
        /// 暗号文の書き込み先となる <see cref="byte"/>スパン。
        /// 少なくとも <paramref name="plainBytes"/> の長さと <see cref="OverheadSize"/> を加算した値以上の長さが必要です。
        /// </param>
        /// <param name="bytesWritten">
        /// 実際に <paramref name="destination"/> に書き込まれたバイトサイズ。
        /// <paramref name="plainBytes"/> の長さと <see cref="OverheadSize"/> を加算した値になります。
        /// </param>
        /// <returns>
        /// 暗号化に成功した場合は <c>true</c>。
        /// <paramref name="plainBytes"/> の長さが <see cref="MaxPayloadSize"/> を超えているか、
        /// <paramref name="destination"/> の長さが <paramref name="plainBytes"/> の長さと <see cref="OverheadSize"/> を加算した値を満たしていない場合は <c>false</c>。
        /// </returns>
        public bool TryEncrypt(ReadOnlySpan<byte> plainBytes, ReadOnlySpan<byte> inputIV, Span<byte> destination, out int bytesWritten) {
            if (plainBytes.Length > MaxPayloadSize) { goto Failure; }
            if (destination.Length < plainBytes.Length + OverheadSize) { goto Failure; }

            var iv = destination.Slice(0, IVSize);
            if (inputIV.Length < IVSize) {
                inputIV.CopyTo(iv);
                iv.Slice(inputIV.Length).Fill(0);
            }
            else {
                inputIV.Slice(0, IVSize).CopyTo(iv);
            }

            var payload = destination.Slice(IVSize, plainBytes.Length);
            plainBytes.CopyTo(payload);
            XorPayload(iv, payload);

            var signature = destination.Slice(IVSize + plainBytes.Length, SignatureSize);
            ComputeSignature(iv, plainBytes, signature);

            bytesWritten = IVSize + payload.Length + SignatureSize;
            return true;

Failure:
            bytesWritten = 0;
            return false;
        }

        /// <summary>
        /// <see cref="ABCrypto"/> で暗号化された暗号文を復号化します。
        /// </summary>
        /// <param name="cipherBytes">復号化対象の暗号文。</param>
        /// <param name="destination">復号化された平文の書き込み先となる <see cref="byte"/> スパン。</param>
        /// <param name="bytesWritten">
        /// 実際に <paramref name="destination"/> に書き込まれたバイトサイズ。
        /// <paramref name="cipherBytes"/> の長さから <see cref="OverheadSize"/> を減算した値になります。
        /// </param>
        /// <returns>
        /// 復号化に成功した場合は <c>true</c>、失敗した場合は <c>false</c> となります。
        /// 失敗するケースは以下の通りです:
        /// <list type="bullet">
        ///     <term><paramref name="cipherBytes"/> の長さが <see cref="OverheadSize"/> を満たさない。</term>
        ///     <term><paramref name="cipherBytes"/> に含まれるペイロードの長さが <see cref="MaxPayloadSize"/> を超えている。</term>
        ///     <term><paramref name="destination"/> の長さがペイロードの長さを満たさない。</term>
        ///     <term>復号化された平文から作成した署名が、<paramref name="cipherBytes"/> に含まれる署名と一致しない。</term>
        /// </list>
        /// </returns>
        public bool TryDecrypt(ReadOnlySpan<byte> cipherBytes, Span<byte> destination, out int bytesWritten) {
            var payloadSize = cipherBytes.Length - OverheadSize;
            if (payloadSize < 0) { goto Failure; }
            if (payloadSize > MaxPayloadSize) { goto Failure; }

            var iv = cipherBytes.Slice(0, IVSize);
            var payload = cipherBytes.Slice(IVSize, payloadSize);
            var signature = cipherBytes.Slice(IVSize + payloadSize, SignatureSize);

            if (!payload.TryCopyTo(destination)) { goto Failure; }
            XorPayload(iv, destination);

            Span<byte> computedSignature = stackalloc byte[SignatureSize];
            ComputeSignature(iv, destination, computedSignature);
            if (!signature.SequenceEqual(computedSignature)) { goto Failure; }

            bytesWritten = payload.Length;
            return true;

Failure:
            bytesWritten = 0;
            return false;
        }

        /// <summary>
        /// ペイロードに XOR 暗号を適用します。
        /// </summary>
        /// <param name="iv">暗号化に使用される初期化ベクトル。</param>
        /// <param name="payload">XOR 暗号を適用するペイロード。</param>
        private void XorPayload(ReadOnlySpan<byte> iv, Span<byte> payload) {
            Debug.Assert(iv.Length == IVSize);
            Debug.Assert(payload.Length <= MaxPayloadSize);

            const int maxCounterSize = 3;

            int sectionCount = (payload.Length + _sectionSize - 1) / _sectionSize;
            Debug.Assert(sectionCount <= _maxSections);

            var padMessage = ArrayPool<byte>.Shared.Rent(iv.Length + maxCounterSize);
            try {
                iv.CopyTo(padMessage);
                var counter = padMessage.AsSpan(iv.Length, maxCounterSize);
                counter.Fill(0);

                var counterSize = 0;
                using (var hmac = new HMACSHA1(_keys.EncryptionKey)) {
                    for (var sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++) {
                        var sectionOffset = sectionIndex * _sectionSize;
                        var section = payload.Slice(sectionOffset, Math.Min(payload.Length - sectionOffset, _sectionSize));

                        var pad = hmac.ComputeHash(padMessage, 0, iv.Length + counterSize);

                        for (var i = 0; i < section.Length; i++) {
                            section[i] ^= pad[i];
                        }

                        if (counterSize == 0 || ++counter[counterSize - 1] == 0) {
                            counterSize++;
                        }
                    }
                }
            }
            finally {
                ArrayPool<byte>.Shared.Return(padMessage);
            }
        }

        /// <summary>
        /// 平文の署名を作成します。
        /// </summary>
        /// <param name="iv">署名に使用される初期化ベクトル。</param>
        /// <param name="playinBytes">署名対象の平文。</param>
        /// <param name="destination">書名の書き込み先となる <see cref="byte"/> スパン。</param>
        private void ComputeSignature(ReadOnlySpan<byte> iv, ReadOnlySpan<byte> playinBytes, Span<byte> destination) {
            Debug.Assert(iv.Length == IVSize);
            Debug.Assert(playinBytes.Length <= MaxPayloadSize);
            Debug.Assert(destination.Length == SignatureSize);

            using (var hmac = new HMACSHA1(_keys.IntegrityKey)) {
                var message = ArrayPool<byte>.Shared.Rent(playinBytes.Length + iv.Length);
                try {
                    playinBytes.CopyTo(message);
                    iv.CopyTo(message.AsSpan(playinBytes.Length));

                    var signature = hmac.ComputeHash(message, 0, playinBytes.Length + iv.Length).AsSpan(0, SignatureSize);
                    signature.CopyTo(destination);
                }
                finally {
                    ArrayPool<byte>.Shared.Return(message, clearArray: true);
                }
            }
        }
    }
}
