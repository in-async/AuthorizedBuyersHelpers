using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Inasync;

namespace AuthorizedBuyersHelpers {

    /// <summary>
    /// Authorized Buyers の <c>AUCTION_PRICE</c> を暗号化・復号化するサービス。
    /// </summary>
    /// <remarks>
    /// [Decrypt Price Confirmations](https://developers.google.com/authorized-buyers/rtb/response-guide/decrypt-price)
    /// </remarks>
    public sealed class ABPriceCrypto {
        private readonly IABCryptoKeys _keys;

        /// <summary>
        /// <see cref="ABPriceCrypto"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="keys"><c>AUCTION_PRICE</c> の暗号化または復号化に必要なキーセット。</param>
        /// <exception cref="ArgumentNullException"><paramref name="keys"/> is <c>null</c>.</exception>
        public ABPriceCrypto(IABCryptoKeys keys) {
            _keys = keys ?? throw new ArgumentNullException(nameof(keys));
        }

        /// <summary>
        /// Authorized Buyers <c>AUCTION_PRICE</c> を復号します。
        /// </summary>
        /// <param name="cipherPrice">Authorized Buyers の <c>AUCTION_PRICE</c>。</param>
        /// <param name="price">復号された <c>AUCTION_PRICE</c>。</param>
        /// <returns>復号に成功すれば <c>true</c>、それ以外なら <c>false</c>。</returns>
        public bool TryDecrypt(string cipherPrice, out decimal price) {
            if (string.IsNullOrEmpty(cipherPrice)) { goto Failure; }

            if (!Base64Url.TryDecode(cipherPrice, out var encPrice)) { goto Failure; }
            if (encPrice.Length != 28) { goto Failure; }

            var iv = new ArraySegment<byte>(encPrice, 0, 16);
            var p = new ArraySegment<byte>(encPrice, 16, 8);
            var sig = new ArraySegment<byte>(encPrice, 24, 4);

            byte[] microPrice;
            using (var hmac = new HMACSHA1(_keys.EncryptionKey)) {
                var pricePad = hmac.ComputeHash(iv.Array, iv.Offset, iv.Count).Take(8).ToArray();

                if (!TryXor(p, pricePad, out microPrice)) { goto Failure; }
            }

            using (var hmac = new HMACSHA1(_keys.IntegrityKey)) {
                var buff = new byte[microPrice.Length + iv.Count];
                Buffer.BlockCopy(microPrice, 0, buff, 0, microPrice.Length);
                Buffer.BlockCopy(iv.Array, iv.Offset, buff, microPrice.Length, iv.Count);
                var confSig = hmac.ComputeHash(buff).Take(4);

                var success = sig.SequenceEqual(confSig);
                if (!success) { goto Failure; }
            }

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(microPrice);
            }
            price = (decimal)BitConverter.ToInt64(microPrice, 0) / 1000000;
            return true;

Failure:
            price = 0;
            return false;
        }

        private static bool TryXor(IReadOnlyList<byte> x, IReadOnlyList<byte> y, out byte[] results) {
            if (x.Count != y.Count) {
                results = null;
                return false;
            }

            results = new byte[x.Count];
            for (var i = 0; i < results.Length; i++) {
                results[i] = (byte)(x[i] ^ y[i]);
            }
            return true;
        }
    }
}
