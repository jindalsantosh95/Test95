using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static readonly HttpRequestOptionsKey<CookieContainer> CookieContainerKey = new("VAA.EAPI.GraphQL.Common.Http.CookieContainer");

        public static HttpRequestMessage Clone(this HttpRequestMessage source) => source.Clone(source.RequestUri);
        public static HttpRequestMessage Clone(this HttpRequestMessage source, Uri location)
        {
            var httpRequestMessage = new HttpRequestMessage(source.Method, location);

            if (source.Content != null)
            {
                var memoryStream = new MemoryStream();
                var stream = source.Content.ReadAsStream();

                stream.CopyTo(memoryStream);

                memoryStream.Position = 0;
                stream.Position = 0;

                httpRequestMessage.Content = new StreamContent(memoryStream);

                if (source.Content.Headers != null)
                    foreach (var header in source.Content.Headers)
                        httpRequestMessage.Content.Headers.Add(header.Key, header.Value);
            }

            httpRequestMessage.Version = source.Version;

            foreach (var option in source.Options)
                httpRequestMessage.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);

            foreach (var header in source.Headers)
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return httpRequestMessage;
        }

        public static string BuildKey(this HttpRequestMessage source)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine($"{source.Method} {source.RequestUri} HTTP/{source.Version}");

            foreach (var header in source.Headers)
                stringBuilder.AppendLine($"{header.Key} {string.Join(' ', header.Value)}");

            if (source.Content is not null)
            {
                var stream = source.Content.ReadAsStream();

                var buffer = new byte[stream.Length];

                stream.Read(buffer, 0, (int)stream.Length);

                stringBuilder.AppendLine(Encoding.UTF8.GetString(buffer));
            }

            return stringBuilder.ToString();
        }

        public static void SetCookieContainer(this HttpRequestMessage request, CookieContainer cookieContainer)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (cookieContainer is null) return;

            request.Options.Set(CookieContainerKey, cookieContainer);
        }

#nullable enable

        public static bool TryGetCookieContainer(this HttpRequestMessage request, [NotNullWhen(true)] out CookieContainer? cookieContainer)
        {
            cookieContainer = null;

            return request is not null && request.Options.TryGetValue(CookieContainerKey, out cookieContainer);
        }

#nullable restore
    }
}
