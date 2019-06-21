using System;
using Inasync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuthorizedBuyersHelpers.Tests {

    [TestClass]
    public class ABCryptoPriceExtensionsTests {

        [TestMethod]
        public void EncryptPrice_AutoIV() {
            Action TestCase(int testNumber, ABCrypto crypto, decimal price, Type expectedExceptionType = null) => () => {
                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => {
                        var cipher = ABCryptoPriceExtensions.EncryptPrice(crypto, price);
                        Console.WriteLine(cipher);
                        return (success: crypto.TryDecryptPrice(cipher, out var actualPrice), price: actualPrice);
                    })
                    .Verify((actual, desc) => {
                        actual.success.Is(true, desc);
                        actual.price.Is(price, desc);
                    }, expectedExceptionType);
            };

            using (var crypto = new ABCrypto(new StubABCryptoKeys(
                  eKey: Rand.Bytes()
                , iKey: Rand.Bytes()
            ))) {
                new[] {
                    TestCase( 0, null  , 0m, typeof(ArgumentNullException)),
                    TestCase( 0, crypto, 0m     ),
                    TestCase( 1, crypto, -1.2m  ),
                    TestCase( 2, crypto, 0.123m ),
                    TestCase( 3, crypto, 1.2m   ),
                    TestCase( 4, crypto, 123.45m),
                    TestCase(10, crypto, (decimal)Rand.Long() / 1_000_000),
                }.Run(parallelPerAction: 10);
            }
        }

        [TestMethod]
        public void EncryptPrice() {
            Action TestCase(int testNumber, ABCrypto crypto, decimal price, byte[] inputIV, string expected, Type expectedExceptionType = null) => () => {
                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => ABCryptoPriceExtensions.EncryptPrice(crypto, price, inputIV))
                    .Verify(expected, expectedExceptionType);
            };

            var iv = Base16.Decode("386E3AC0000C0A080123456789ABCDEF");
            var shortIV = Base16.Decode("386E3AC0000C0A080123456789ABCD");
            var longIV = Base16.Decode("386E3AC0000C0A080123456789ABCDEF01");
            new[] {
                TestCase( 0, null   , 0m     , iv     , default, typeof(ArgumentNullException)),
                TestCase( 1, _crypto, 0m     , iv     , "OG46wAAMCggBI0VniavN7-mNy0VTOrlB8A3F8A=="),
                TestCase( 2, _crypto, -1.2m  , iv     , "OG46wAAMCggBI0VniavN7xZyNLqs1wnBKd_m9w=="),
                TestCase( 3, _crypto, 0.123m , iv     , "OG46wAAMCggBI0VniavN7-mNy0VTO1k5XIpbSg=="),
                TestCase( 4, _crypto, 1.2m   , iv     , "OG46wAAMCggBI0VniavN7-mNy0VTKPbB3o5CMQ=="),
                TestCase( 5, _crypto, 123.45m, iv     , "OG46wAAMCggBI0VniavN7-mNy0VUYQvRuIJiRw=="),
                TestCase( 6, _crypto, 123.45m, shortIV, "OG46wAAMCggBI0VniavNAF7WOkwZcM1moYKxlA=="),
                TestCase( 7, _crypto, 123.45m, longIV , "OG46wAAMCggBI0VniavN7-mNy0VUYQvRuIJiRw=="),
                TestCase(10, _crypto, (decimal)long.MinValue / 1_000_000, iv, "OG46wAAMCggBI0VniavN72mNy0VTOrlBMgsdYg=="),
                TestCase(11, _crypto, (decimal)long.MinValue / 1_000_000 - 0.000001m, iv, default, typeof(OverflowException)),
                TestCase(12, _crypto, (decimal)long.MaxValue / 1_000_000, iv, "OG46wAAMCggBI0VniavN75ZyNLqsxUa-p2RaNA=="),
                TestCase(13, _crypto, (decimal)long.MaxValue / 1_000_000 + 1, iv, default, typeof(OverflowException)),
            }.Run(parallelPerAction: 10);
        }

        [TestMethod]
        public void TryDecryptPrice() {
            Action TestCase(int testNumber, ABCrypto crypto, string cipherPrice, (bool, decimal) expected, Type expectedExceptionType = null) => () => {
                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => (ABCryptoPriceExtensions.TryDecryptPrice(crypto, cipherPrice, out var price), price))
                    .Verify(expected, expectedExceptionType);
            };

            new[] {
                TestCase( 0, null   , default                                   , default, typeof(ArgumentNullException)),
                TestCase( 1, _crypto, null                                      , (false, 0)),
                TestCase( 2, _crypto, ""                                        , (false, 0)),
                TestCase( 3, _crypto, "A"                                       , (false, 0)),
                TestCase( 4, _crypto, "AA"                                      , (false, 0)),
                TestCase( 5, _crypto, "OG46wAAMCggBI0VniavN7-mNy0VTKPbB3o5CMQ==", (true , 1.2m)),
                TestCase( 6, _crypto, "5nmwvgAM0UABI0VniavN72_sy3T6V9ohlpvOpA==", (true , 1.2m)),
                TestCase( 7, _crypto, "5nmwvgAM0UABI0VniavN72_sy3T6V9oglpvOpA==", (false, 0)),  // ペイロード改竄。
                TestCase( 8, _crypto, "OG46wAAMCggBI0VniavN7-mNy0VUYQvRuIJiRw==", (true , 123.45m)),
            }.Run(parallelPerAction: 10);
        }

        #region Helpers

        private static readonly ABCrypto _crypto = new ABCrypto(new StubABCryptoKeys(
              eKey: Base64Url.Decode("sIxwz7yw62yrfoLGt12lIHKuYrK_S5kLuApI2BQe7Ac=")
            , iKey: Base64Url.Decode("v3fsVcMBMMHYzRhi7SpM0sdqwzvAxM6KPTu9OtVod5I=")
        ));

        [ClassCleanup]
        public static void ClassCleanup() {
            _crypto.Dispose();
        }

        #endregion Helpers
    }
}
