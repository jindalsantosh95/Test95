using System;
using System.IO;
using System.IO.Compression;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class ByteExtensions
    {
        public static string DecompressToString(this byte[] source)
        {
            if (source.IsNullOrEmpty()) throw new ArgumentNullException(nameof(source));

            return System.Text.Encoding.UTF8.GetString(source.Decompress());
        }
        public static T DecompressTo<T>(this byte[] source)
        {
            if (source.IsNullOrEmpty()) throw new ArgumentNullException(nameof(source));

            return Json.Deserialize<T>(source.DecompressToString());
        }

        public static byte[] Decompress(this byte[] source)
        {
            if (source.IsNullOrEmpty()) throw new ArgumentNullException(nameof(source));

            source = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(source));

            var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(new MemoryStream(source) { Position = 0 }, CompressionMode.Decompress);

            gZipStream.CopyTo(memoryStream);

            gZipStream.Flush();

            memoryStream.Position = 0;

            return memoryStream.ToArray();
        }
    }
}
