using System;
using System.IO;
using System.IO.Compression;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Common.Extensions
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

            try
            {
                // legacy support for any items in cache stored as base64
                if (System.Buffers.Text.Base64.IsValid(source))
                    source = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(source));
            }
            catch { }

            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(new MemoryStream(source) { Position = 0 }, CompressionMode.Decompress);

            gZipStream.CopyTo(memoryStream);

            gZipStream.Flush();

            return memoryStream.ToArray();
        }

        public static byte[] Compress(this byte[] source)
        {
            if (source.IsNullOrEmpty()) throw new ArgumentNullException(nameof(source));

            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);

            gZipStream.Write(source, 0, source.Length);

            gZipStream.Flush();

            return memoryStream.ToArray();
        }
    }
}
