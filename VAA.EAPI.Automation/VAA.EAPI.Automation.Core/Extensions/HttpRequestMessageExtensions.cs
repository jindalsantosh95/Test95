using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage source) => source.Clone(source.RequestUri);
        public static HttpRequestMessage Clone(this HttpRequestMessage source, Uri location)
        {
            System.Net.Http.HttpRequestMessage httpRequestMessage = new HttpRequestMessage(source.Method, location);

            if (source.Content != null)
            {
                MemoryStream memoryStream = new MemoryStream();

                source.Content.ReadAsStream().CopyTo(memoryStream);

                memoryStream.Position = 0;

                httpRequestMessage.Content = new StreamContent(memoryStream);

                if (source.Content.Headers != null)
                    foreach (KeyValuePair<string, IEnumerable<string>> header in source.Content.Headers)
                        httpRequestMessage.Content.Headers.Add(header.Key, header.Value);
            }

            httpRequestMessage.Version = source.Version;

            foreach (KeyValuePair<string, object?> option in source.Options)
                httpRequestMessage.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);

            foreach (KeyValuePair<string, IEnumerable<string>> header in source.Headers)
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return httpRequestMessage;
        }

        public static string BuildKey(this HttpRequestMessage source)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine($"{source.Method} {source.RequestUri} HTTP/{source.Version}");

            foreach (KeyValuePair<string, IEnumerable<string>> header in source.Headers)
                stringBuilder.AppendLine($"{header.Key} {string.Join(' ', header.Value)}");

            if (source.Content is not null)
            {
                Stream stream = source.Content.ReadAsStream();

                using MemoryStream contentBuffer = new();
                stream.CopyTo(contentBuffer);
                byte[] buffer = contentBuffer.ToArray();

                stringBuilder.AppendLine(Encoding.UTF8.GetString(buffer));
            }

            return stringBuilder.ToString();
        }
    }
}
