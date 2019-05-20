using System;
using Inasync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuthorizedBuyersHelpers.Tests {

    [TestClass]
    public class ABPriceCryptoTests {

        [TestMethod]
        public void Ctor() {
            Action TestCase(int testNumber, IABCryptoKeys keys, Type expectedExceptionType) => () => {
                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => new ABPriceCrypto(keys))
                    .Verify((_, __) => { }, expectedExceptionType);
            };

            new[] {
                TestCase( 0, null                  , typeof(ArgumentNullException)),
                TestCase( 1, new StubABCryptoKeys(), null)
            }.Run();
        }

        [TestMethod]
        public void TryDecrypt() {
            var crypto = new ABPriceCrypto(new StubABCryptoKeys(
                  eKey: Base64Url.Decode("sIxwz7yw62yrfoLGt12lIHKuYrK_S5kLuApI2BQe7Ac=")
                , iKey: Base64Url.Decode("v3fsVcMBMMHYzRhi7SpM0sdqwzvAxM6KPTu9OtVod5I=")
            ));

            Action TestCase(int testNumber, string cipherPrice, (bool, decimal) expected) => () => {
                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => (crypto.TryDecrypt(cipherPrice, out var price), price))
                    .Verify(expected, null);
            };

            new[] {
                TestCase( 0, null                                      , (false, 0)),
                TestCase( 1, ""                                        , (false, 0)),
                TestCase( 2, "A"                                       , (false, 0)),
                TestCase( 3, "AA"                                      , (false, 0)),
                TestCase( 4, "5nmwvgAM0UABI0VniavN72_sy3T6V9ohlpvOpA==", (true , 1.2m)),
            }.Run();
        }

        #region Helpers

        private sealed class StubABCryptoKeys : IABCryptoKeys {

            public StubABCryptoKeys() {
            }

            public StubABCryptoKeys(byte[] eKey, byte[] iKey) {
                EncryptionKey = eKey ?? throw new ArgumentNullException(nameof(eKey));
                IntegrityKey = iKey ?? throw new ArgumentNullException(nameof(iKey));
            }

            public byte[] EncryptionKey { get; }
            public byte[] IntegrityKey { get; }
        }

        #endregion Helpers
    }
}
