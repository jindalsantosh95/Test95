using System.IO.Compression;
using VAA.EAPI.Automation.Core.Serialisation;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class ByteExtensions
    {
        public static string DecompressToString(this byte[] source)
        {
            if (source.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(source));

            return System.Text.Encoding.UTF8.GetString(source.Decompress());
        }
        public static T DecompressTo<T>(this byte[] source)
        {
            if (source.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(source));

            return Json.Deserialize<T>(source.DecompressToString());
        }

        public static byte[] Decompress(this byte[] source)
        {
            if (source.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(source));

            try
            {
                // legacy support for any items in cache stored as base64
                if (System.Buffers.Text.Base64.IsValid(source))
                    source = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(source));
            }
            catch { }

            using MemoryStream memoryStream = new MemoryStream();
            using GZipStream gZipStream = new GZipStream(new MemoryStream(source) { Position = 0 }, CompressionMode.Decompress);

            gZipStream.CopyTo(memoryStream);

            gZipStream.Flush();

            return memoryStream.ToArray();
        }

        public static byte[] Compress(this byte[] source)
        {
            if (source.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(source));

            using MemoryStream memoryStream = new MemoryStream();

            // Dispose the GZipStream before reading the buffer so the gzip footer
            // (final block + CRC/length trailer) is written; leaveOpen keeps the
            // MemoryStream open for ToArray().
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
            {
                gZipStream.Write(source, 0, source.Length);
            }

            return memoryStream.ToArray();
        }
    }
}
