using System;
using Inasync;

namespace AuthorizedBuyersHelpers {

    internal static class ABCipherEncoder {

        public static string Encode(ArraySegment<byte> cipherBytes) => Base64Url.Encode(cipherBytes.Array, cipherBytes.Offset, cipherBytes.Count, padding: true);

        public static bool TryDecode(string cipherText, out byte[] cipherBytes) => Base64Url.TryDecode(cipherText, out cipherBytes);
    }
}
