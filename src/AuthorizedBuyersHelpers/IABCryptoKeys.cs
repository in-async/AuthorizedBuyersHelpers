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
}
