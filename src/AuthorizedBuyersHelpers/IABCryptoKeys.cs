using System;

namespace AuthorizedBuyersHelpers {

    /// <summary>
    /// Authorized Buyers の暗号化スキームに必要なキーセット。
    /// </summary>
    /// <remarks>
    /// https://developers.google.com/authorized-buyers/rtb/response-guide/decrypt-price
    /// https://github.com/google/openrtb-doubleclick/wiki#cryptography
    /// </remarks>
    public interface IABCryptoKeys {

        /// <summary>
        /// 暗号化キー。常に非 <c>null</c>。
        /// </summary>
        byte[] EncryptionKey { get; }

        /// <summary>
        /// 整合性キー。常に非 <c>null</c>。
        /// </summary>
        byte[] IntegrityKey { get; }
    }

    /// <summary>
    /// <see cref="IABCryptoKeys"/> の実装クラス。
    /// </summary>
    public class ABCryptoKeys : IABCryptoKeys {

        /// <summary>
        /// <see cref="ABCryptoKeys"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="encryptionKey"><see cref="EncryptionKey"/> に渡される値。</param>
        /// <param name="integrityKey"><see cref="IntegrityKey"/> に渡される値。</param>
        /// <exception cref="ArgumentNullException"><paramref name="encryptionKey"/> or <paramref name="integrityKey"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="encryptionKey"/> or <paramref name="integrityKey"/> is empty.</exception>
        public ABCryptoKeys(byte[] encryptionKey, byte[] integrityKey) {
            EncryptionKey = encryptionKey ?? throw new ArgumentNullException(nameof(encryptionKey));
            if (encryptionKey.Length == 0) { throw new ArgumentException($"{nameof(EncryptionKey)} is empty.", nameof(encryptionKey)); }

            IntegrityKey = integrityKey ?? throw new ArgumentNullException(nameof(integrityKey));
            if (integrityKey.Length == 0) { throw new ArgumentException($"{nameof(IntegrityKey)} is empty.", nameof(integrityKey)); }
        }

        /// <summary>
        /// <see cref="IABCryptoKeys.EncryptionKey"/> の実装。
        /// </summary>
        public byte[] EncryptionKey { get; }

        /// <summary>
        /// <see cref="IABCryptoKeys.IntegrityKey"/> の実装。
        /// </summary>
        public byte[] IntegrityKey { get; }
    }
}
