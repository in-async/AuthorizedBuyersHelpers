using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;

namespace AuthorizedBuyersHelpers {

    /// <summary>
    /// Authorized Buyers の落札価格を暗号化・復号化する <see cref="ABCrypto"/> の拡張メソッドを提供するクラス。
    /// </summary>
    /// <remarks>
    /// https://developers.google.com/authorized-buyers/rtb/response-guide/decrypt-price
    /// </remarks>
    public static class ABPriceCrypto {

        /// <summary>
        /// 暗号化された価格のバイトサイズ。
        /// </summary>
        public const int CipherSize = ABCrypto.OverheadSize + PricePayloadSize;

        private const int PricePayloadSize = 8;
        private const int MicrosPerCurrencyUnit = 1_000_000;

        /// <summary>
        /// 価格を暗号化します。
        /// </summary>
        /// <param name="crypto">暗号化オブジェクト。</param>
        /// <param name="price">暗号化対象の価格。</param>
        /// <returns>暗号化された価格を表す暗号文。常に非 <c>null</c>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="crypto"/> is <c>null</c>.</exception>
        public static string EncryptPrice(this ABCrypto crypto, decimal price) {
            if (crypto == null) { throw new ArgumentNullException(nameof(crypto)); }

            Span<byte> iv = stackalloc byte[ABCrypto.IVSize];
            var success = ABIV.TryCreate(iv, out _);
            Debug.Assert(success);

            return EncryptPrice(crypto, price, iv);
        }

        /// <summary>
        /// 価格を暗号化します。
        /// </summary>
        /// <param name="crypto">暗号化オブジェクト。</param>
        /// <param name="price">暗号化対象の価格。</param>
        /// <param name="inputIV">
        /// 暗号化に使用される初期化ベクトル。
        /// 長さが <see cref="ABCrypto.IVSize"/> に満たない場合は不足分を 0 で埋め、<see cref="ABCrypto.IVSize"/> を超える場合は <see cref="ABCrypto.IVSize"/> まで切り詰めて使用します。
        /// </param>
        /// <returns>暗号化された価格を表す暗号文。常に非 <c>null</c>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="crypto"/> is <c>null</c>.</exception>
        /// <exception cref="OverflowException">
        /// <paramref name="price"/> が <c><see cref="long.MaxValue"/> / 1_000_000</c> より大きい、
        /// または <c><see cref="long.MinValue"/> / 1_000_000 より小さい。</c>
        /// </exception>
        public static string EncryptPrice(this ABCrypto crypto, decimal price, ReadOnlySpan<byte> inputIV) {
            if (crypto == null) { throw new ArgumentNullException(nameof(crypto)); }

            var cipherBytes = ArrayPool<byte>.Shared.RentAsSegment(CipherSize);
            try {
                var success = TryEncryptPrice(crypto, price, inputIV, cipherBytes, out _);
                Debug.Assert(success);

                return ABCipherEncoder.Encode(cipherBytes);
            }
            finally {
                ArrayPool<byte>.Shared.Return(cipherBytes.Array);
            }
        }

        /// <summary>
        /// 価格を暗号化します。
        /// </summary>
        /// <param name="crypto">暗号化オブジェクト。</param>
        /// <param name="price">暗号化対象の価格。</param>
        /// <param name="inputIV">
        /// 暗号化に使用される初期化ベクトル。
        /// 長さが <see cref="ABCrypto.IVSize"/> に満たない場合は不足分を 0 で埋め、<see cref="ABCrypto.IVSize"/> を超える場合は <see cref="ABCrypto.IVSize"/> まで切り詰めて使用します。
        /// </param>
        /// <param name="cipherBytes">
        /// 暗号データの書き込み先となる <see cref="byte"/> スパン。
        /// 少なくとも <see cref="CipherSize"/> 以上の長さが必要です。
        /// </param>
        /// <param name="bytesWritten">
        /// 実際に <paramref name="cipherBytes"/> に書き込まれたバイトサイズ。
        /// 暗号化に成功した場合は <see cref="CipherSize"/> の値になります。
        /// </param>
        /// <returns>
        /// 暗号化に成功した場合は <c>true</c>。
        /// <paramref name="cipherBytes"/> の長さが <see cref="CipherSize"/> を満たしていない場合は <c>false</c>。
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="crypto"/> is <c>null</c>.</exception>
        /// <exception cref="OverflowException">
        /// <paramref name="price"/> が <c><see cref="long.MaxValue"/> / 1_000_000</c> より大きい、
        /// または <c><see cref="long.MinValue"/> / 1_000_000 より小さい。</c>
        /// </exception>
        public static bool TryEncryptPrice(this ABCrypto crypto, decimal price, ReadOnlySpan<byte> inputIV, Span<byte> cipherBytes, out int bytesWritten) {
            if (crypto == null) { throw new ArgumentNullException(nameof(crypto)); }
            if (cipherBytes.Length < CipherSize) { goto Failure; }

            Span<byte> microPriceData = stackalloc byte[PricePayloadSize];
            BinaryPrimitives.WriteInt64BigEndian(microPriceData, (long)(price * MicrosPerCurrencyUnit));

            var success = crypto.TryEncrypt(microPriceData, inputIV, cipherBytes, out bytesWritten);
            Debug.Assert(success);
            Debug.Assert(bytesWritten == CipherSize);
            return true;

Failure:
            bytesWritten = 0;
            return false;
        }

        /// <summary>
        /// <see cref="ABCrypto"/> によって暗号化された暗号文を価格に復号化します。
        /// </summary>
        /// <param name="crypto">暗号化オブジェクト。</param>
        /// <param name="cipherPrice">暗号化された価格を表す暗号文。</param>
        /// <param name="price">復号化された価格。</param>
        /// <returns>復号化に成功した場合は <c>true</c>、それ以外なら <c>false</c>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="crypto"/> is <c>null</c>.</exception>
        public static bool TryDecryptPrice(this ABCrypto crypto, string cipherPrice, out decimal price) {
            if (crypto == null) { throw new ArgumentNullException(nameof(crypto)); }

            if (!ABCipherEncoder.TryDecode(cipherPrice, out var cipherBytes)) { goto Failure; }

            return TryDecryptPrice(crypto, cipherBytes, out price);

Failure:
            price = default;
            return false;
        }

        /// <summary>
        /// <see cref="ABCrypto"/> によって暗号化された価格を復号化します。
        /// </summary>
        /// <param name="crypto">暗号化オブジェクト。</param>
        /// <param name="cipherBytes">暗号化された価格を表す暗号データ。</param>
        /// <param name="price">復号化された価格。</param>
        /// <returns>復号化に成功した場合は <c>true</c>、それ以外なら <c>false</c>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="crypto"/> is <c>null</c>.</exception>
        public static bool TryDecryptPrice(this ABCrypto crypto, ReadOnlySpan<byte> cipherBytes, out decimal price) {
            if (crypto == null) { throw new ArgumentNullException(nameof(crypto)); }
            if (cipherBytes.Length != CipherSize) { goto Failure; }

            Span<byte> microPriceData = stackalloc byte[PricePayloadSize];
            if (!crypto.TryDecrypt(cipherBytes, microPriceData, out _)) { goto Failure; }

            price = (decimal)BinaryPrimitives.ReadInt64BigEndian(microPriceData) / MicrosPerCurrencyUnit;
            return true;

Failure:
            price = default;
            return false;
        }
    }
}
