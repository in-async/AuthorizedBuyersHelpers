using System;
using System.Buffers.Binary;
using System.Globalization;
using Inasync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuthorizedBuyersHelpers.Tests {

    [TestClass]
    public class ABIVTests {

        [TestMethod]
        public void TryCreate_Auto() {
            Action TestCase(int testNumber, byte[] destination, bool expected, Type expectedExceptionType = null) => () => {
                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => ABIV.TryCreate(destination))
                    .Verify(expected, expectedExceptionType);
            };

            new[]{
                TestCase( 0, new byte[0] , false),
                TestCase( 1, new byte[15], false),
                TestCase( 2, new byte[16], true ),
                TestCase( 3, new byte[17], true ),
            }.Run();
        }

        [TestMethod]
        public void TryCreate() {
            Action TestCase(int testNumber, DateTime date, long serverId, byte[] destination, (bool success, uint seconds, uint micros) expected, Type expectedExceptionType = null) => () => {
                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => ABIV.TryCreate(date, serverId, destination))
                    .Verify((actual, desc) => {
                        actual.Is(expected.success, desc);

                        if (actual) {
                            BinaryPrimitives.ReadUInt32BigEndian(destination.AsSpan(0, 4)).Is(expected.seconds, desc);
                            BinaryPrimitives.ReadUInt32BigEndian(destination.AsSpan(4, 4)).Is(expected.micros, desc);
                            BinaryPrimitives.ReadInt64BigEndian(destination.AsSpan(8, 8)).Is(serverId, desc);
                        }
                    }, expectedExceptionType);
            };

            var now = DateTime.Parse("2019-06-17T07:33:12.2270981Z", null, DateTimeStyles.AdjustToUniversal);
            new[]{
                TestCase( 0, now, (long)(Rand.Next() * long.MaxValue), new byte[0] , (false, default, default)),
                TestCase( 1, now, (long)(Rand.Next() * long.MaxValue), new byte[15], (false, default, default)),
                TestCase( 2, now, (long)(Rand.Next() * long.MaxValue), new byte[16], (true , 1560756792, 227098)),
                TestCase( 3, now, (long)(Rand.Next() * long.MaxValue), new byte[17], (true , 1560756792, 227098)),
                TestCase(10, new DateTime(1969, 12, 31, 23, 59, 59, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), new byte[16], (false, default, default)),  // UNIX Epoch より古い日時。
                TestCase(11, new DateTime(1970,  1,  1,  0,  0,  0, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), new byte[16], (true , 0, 0)),  // UNIX Epoch。
                TestCase(12, new DateTime(1970,  1,  1,  0,  0,  1, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), new byte[16], (true , 1, 0)),  // UNIX Epoch + 1s。
                TestCase(13, new DateTime(2038,  1, 19,  3, 14,  8, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), new byte[16], (true , (uint)int.MaxValue + 1, 0)),  // 2038 年問題。
                TestCase(14, new DateTime(2106,  2,  7,  6, 28, 16, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), new byte[16], (true , 0, 0)),  // 2106 年問題。
            }.Run();
        }

        /// <summary>
        /// https://github.com/google/openrtb-doubleclick/blob/2dbbbb54fd7079f107a3dfdd748d2e44e18605c3/doubleclick-core/src/test/java/com/google/doubleclick/crypto/DoubleClickCryptoTest.java#L108-L113
        /// </summary>
        [TestMethod]
        public void TryCreate_Origin() {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = unixEpoch.AddMilliseconds(946748096789L);
            var serverId = 0x0123456789ABCDEFL;

            var destination = new byte[ABCrypto.IVSize];
            new TestCaseRunner()
                .Run(() => ABIV.TryCreate(timestamp, serverId, destination))
                .Verify((actual, _) => {
                    actual.Is(true);
                    destination.Is(Base16.Decode("386E3AC0000C0A080123456789ABCDEF"));
                }, (Type)null);
        }
    }
}
