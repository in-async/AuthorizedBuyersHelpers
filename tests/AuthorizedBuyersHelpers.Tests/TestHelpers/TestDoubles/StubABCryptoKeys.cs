namespace AuthorizedBuyersHelpers.Tests {

    public sealed class StubABCryptoKeys : IABCryptoKeys {

        public StubABCryptoKeys(byte[] eKey = null, byte[] iKey = null) {
            EncryptionKey = eKey;
            IntegrityKey = iKey;
        }

        public byte[] EncryptionKey { get; }
        public byte[] IntegrityKey { get; }
    }
}
