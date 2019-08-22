using System;
using Inasync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuthorizedBuyersHelpers.Tests {

    [TestClass]
    public class ABCryptoKeysTests {

        [TestMethod]
        public void Ctor() {
            Action TestCase(int testNumber, byte[] eKey, byte[] iKey, Type expectedExceptionType = null) => () => {
                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => new ABCryptoKeys(eKey, iKey))
                    .Verify((actual, desc) => {
                        actual.EncryptionKey.Is(eKey, desc);
                        actual.IntegrityKey.Is(iKey, desc);
                    }, expectedExceptionType);
            };

            new[] {
                TestCase( 0, null                    , Rand.Bytes(minLength: 1), typeof(ArgumentNullException)),
                TestCase( 1, new byte[0]             , Rand.Bytes(minLength: 1), typeof(ArgumentException)),
                TestCase( 2, Rand.Bytes(minLength: 1), null                    , typeof(ArgumentNullException)),
                TestCase( 3, Rand.Bytes(minLength: 1), new byte[0]             , typeof(ArgumentException)),
                TestCase(10, Rand.Bytes(minLength: 1), Rand.Bytes(minLength: 1)),
            }.Run();
        }
    }
}
