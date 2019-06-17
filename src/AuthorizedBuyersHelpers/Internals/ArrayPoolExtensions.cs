using System;
using System.Buffers;

namespace AuthorizedBuyersHelpers {

    internal static class ArrayPoolExtensions {

        public static ArraySegment<T> RentAsSegment<T>(this ArrayPool<T> pool, int minimumLength) => new ArraySegment<T>(pool.Rent(minimumLength), 0, minimumLength);
    }
}
