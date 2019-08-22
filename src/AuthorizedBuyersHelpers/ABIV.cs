using System;
using System.Buffers.Binary;
using System.Threading;

namespace AuthorizedBuyersHelpers {

    /// <summary>
    /// Authorized Buyers 固有のフォーマットで生成される初期化ベクトルのファクトリー クラス。
    /// </summary>
    /// <remarks>
    /// https://developers.google.com/authorized-buyers/rtb/response-guide/decrypt-price#detecting_stale
    /// </remarks>
    public static class ABIV {

        /// <summary>
        /// スレッドローカルな <see cref="Random"/>。
        /// </summary>
        private static readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random());

        /// <summary>
        /// 初期化ベクトルを生成します。
        /// </summary>
        /// <param name="destination">初期化ベクトルの書き込み先となる <see cref="byte"/> のスパン。</param>
        /// <returns>
        /// 生成に成功した場合は <c>true</c>。
        /// <paramref name="destination"/> の長さが <see cref="ABCrypto.IVSize"/> に満たない場合は <c>false</c>。
        /// </returns>
        public static bool TryCreate(Span<byte> destination) {
            var rnd = _random.Value;
            var serverId = (long)rnd.Next() << 32 | (long)rnd.Next();
            return TryCreate(DateTime.UtcNow, serverId, destination);
        }

        /// <summary>
        /// 初期化ベクトルを生成します。
        /// </summary>
        /// <param name="date">初期化ベクトルの前段 8 bytes に書き込む日時。うち、上位 4 bytes が秒、下位 4 bytes がマイクロ秒を表す。バイトオーダーは BigEndian。</param>
        /// <param name="serverId">初期化ベクトルの後段 8 bytes に書き込むサーバー ID。バイトオーダーは BigEndian。</param>
        /// <param name="destination">初期化ベクトルの書き込み先となる <see cref="byte"/> のスパン。</param>
        /// <returns>
        /// 生成に成功した場合は <c>true</c>。
        /// <paramref name="destination"/> の長さが <see cref="ABCrypto.IVSize"/> に満たない場合、
        /// または <paramref name="date"/> が <see cref="UnixTime.Epoch"/> より古い日時の場合、<c>false</c> となります。
        /// </returns>
        public static bool TryCreate(DateTime date, long serverId, Span<byte> destination) {
            if (destination.Length < ABCrypto.IVSize) { return false; }

            var microUnixtime = new UnixTime(date).TimeSpan.Ticks / 10;
            if (microUnixtime < 0) { return false; }

            BinaryPrimitives.WriteUInt32BigEndian(destination.Slice(0, 4), (uint)(microUnixtime / 1_000_000));
            BinaryPrimitives.WriteUInt32BigEndian(destination.Slice(4, 4), (uint)(microUnixtime % 1_000_000));
            BinaryPrimitives.WriteInt64BigEndian(destination.Slice(8, 8), serverId);

            return true;
        }
    }
}
