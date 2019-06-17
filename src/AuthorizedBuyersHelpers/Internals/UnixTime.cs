using System;

namespace AuthorizedBuyersHelpers {

    /// <summary>
    /// UNIX 時間を表す構造体。
    /// </summary>
    internal readonly struct UnixTime {

        /// <summary>
        /// UNIX Epoch (1970-01-01T00:00:00Z)。
        /// </summary>
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 現在の UNIX 時間を表す <see cref="UnixTime"/> を返します。
        /// </summary>
        public static UnixTime Now => new UnixTime(DateTime.Now);

        /// <summary>
        /// <see cref="UnixTime"/> 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="date">UNIX 時間の元となる日時。</param>
        public UnixTime(DateTime date) => TimeSpan = date.ToUniversalTime() - Epoch;

        /// <summary>
        /// UNIX 時間を表す秒。
        /// </summary>
        public long Value => (long)TimeSpan.TotalSeconds;

        /// <summary>
        /// UNIX 時間の <see cref="TimeSpan"/> 表現。
        /// </summary>
        public TimeSpan TimeSpan { get; }
    }
}
