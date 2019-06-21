using System;
using System.Buffers.Binary;
using System.Globalization;
using System.Threading.Tasks;
using Inasync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuthorizedBuyersHelpers.Tests {

    [TestClass]
    public class ABIVTests {

        [TestMethod]
        public void TryCreate_Auto() {
            Action TestCase(int testNumber, int destinationLength, (bool success, int bytesWritten) expected, Type expectedExceptionType = null) => () => {
                var destination = new byte[destinationLength];

                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => (success: ABIV.TryCreate(destination, out var bytesWritten), bytesWritten))
                    .Verify((actual, desc) => {
                        actual.success.Is(expected.success, desc);
                        actual.bytesWritten.Is(expected.bytesWritten, desc);
                    }, expectedExceptionType);
            };

            new[]{
                TestCase( 0, 0 , (false, 0)),
                TestCase( 1, 15, (false, 0)),
                TestCase( 2, 16, (true , 16)),
                TestCase( 3, 17, (true , 16)),
            }.Run(parallelPerAction: 10);
        }

        [TestMethod]
        public void TryCreate() {
            Action TestCase(int testNumber, DateTime date, long serverId, int destinationLength, (bool success, int bytesWritten, uint seconds, uint micros) expected, Type expectedExceptionType = null) => () => {
                var destination = new byte[destinationLength];

                new TestCaseRunner($"No.{testNumber}")
                    .Run(() => (success: ABIV.TryCreate(date, serverId, destination, out var bytesWritten), bytesWritten))
                    .Verify((actual, desc) => {
                        actual.success.Is(expected.success, desc);
                        actual.bytesWritten.Is(expected.bytesWritten, desc);

                        if (actual.success) {
                            BinaryPrimitives.ReadUInt32BigEndian(destination.AsSpan(0, 4)).Is(expected.seconds, desc);
                            BinaryPrimitives.ReadUInt32BigEndian(destination.AsSpan(4, 4)).Is(expected.micros, desc);
                            BinaryPrimitives.ReadInt64BigEndian(destination.AsSpan(8, 8)).Is(serverId, desc);
                        }
                    }, expectedExceptionType);
            };

            var now = DateTime.Parse("2019-06-17T07:33:12.2270981Z", null, DateTimeStyles.AdjustToUniversal);
            new[]{
                TestCase( 0, now, (long)(Rand.Next() * long.MaxValue), 0 , (false, 0 , default, default)),
                TestCase( 1, now, (long)(Rand.Next() * long.MaxValue), 15, (false, 0 , default, default)),
                TestCase( 2, now, (long)(Rand.Next() * long.MaxValue), 16, (true , 16, 1560756792, 227098)),
                TestCase( 3, now, (long)(Rand.Next() * long.MaxValue), 17, (true , 16, 1560756792, 227098)),
                TestCase(10, new DateTime(1969, 12, 31, 23, 59, 59, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), 16, (false, 0, default, default)),  // UNIX Epoch より古い日時。
                TestCase(11, new DateTime(1970,  1,  1,  0,  0,  0, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), 16, (true , 16, 0, 0)),  // UNIX Epoch。
                TestCase(12, new DateTime(1970,  1,  1,  0,  0,  1, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), 16, (true , 16, 1, 0)),  // UNIX Epoch + 1s。
                TestCase(13, new DateTime(2038,  1, 19,  3, 14,  8, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), 16, (true , 16, (uint)int.MaxValue + 1, 0)),  // 2038 年問題。
                TestCase(14, new DateTime(2106,  2,  7,  6, 28, 16, DateTimeKind.Utc), (long)(Rand.Next() * long.MaxValue), 16, (true , 16, 0, 0)),  // 2106 年問題。
            }.Run(parallelPerAction: 10);
        }

        /// <summary>
        /// https://github.com/google/openrtb-doubleclick/blob/2dbbbb54fd7079f107a3dfdd748d2e44e18605c3/doubleclick-core/src/test/java/com/google/doubleclick/crypto/DoubleClickCryptoTest.java#L108-L113
        /// </summary>
        [TestMethod]
        public void TryCreate_Origin() {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = unixEpoch.AddMilliseconds(946748096789L);
            var serverId = 0x0123456789ABCDEFL;

            Parallel.For(0, 10, __ => {
                var destination = new byte[ABCrypto.IVSize];

                new TestCaseRunner()
                    .Run(() => ABIV.TryCreate(timestamp, serverId, destination, out _))
                    .Verify((actual, _) => {
                        actual.Is(true);
                        destination.Is(Base16.Decode("386E3AC0000C0A080123456789ABCDEF"));
                    }, (Type)null);
            });
        }
    }
}
