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
    public static class ABCryptoPriceExtensions {
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

            Span<byte> microPriceData = stackalloc byte[PricePayloadSize];
            BinaryPrimitives.WriteInt64BigEndian(microPriceData, (long)(price * MicrosPerCurrencyUnit));

            var cipherBytes = ArrayPool<byte>.Shared.RentAsSegment(ABCrypto.OverheadSize + PricePayloadSize);
            try {
                var success = crypto.TryEncrypt(microPriceData, inputIV, cipherBytes, out _);
                Debug.Assert(success);

                return ABCipherEncoder.Encode(cipherBytes);
            }
            finally {
                ArrayPool<byte>.Shared.Return(cipherBytes.Array);
            }
        }

        /// <summary>
        /// <see cref="ABCrypto"/> で暗号化された暗号文を価格に復号化します。
        /// </summary>
        /// <param name="crypto">暗号化オブジェクト。</param>
        /// <param name="cipherPrice">暗号化された価格を表す暗号文。</param>
        /// <param name="price">復号化された価格。</param>
        /// <returns>復号化に成功した場合は <c>true</c>、それ以外なら <c>false</c>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="crypto"/> is <c>null</c>.</exception>
        public static bool TryDecryptPrice(this ABCrypto crypto, string cipherPrice, out decimal price) {
            if (crypto == null) { throw new ArgumentNullException(nameof(crypto)); }

            if (!ABCipherEncoder.TryDecode(cipherPrice, out var cipherBytes)) { goto Failure; }
            if (cipherBytes.Length != ABCrypto.OverheadSize + PricePayloadSize) { goto Failure; }

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
