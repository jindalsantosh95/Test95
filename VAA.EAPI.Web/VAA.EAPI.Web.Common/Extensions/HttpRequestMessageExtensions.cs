using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace VAA.EAPI.Web.Common.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static readonly HttpRequestOptionsKey<CookieContainer> CookieContainerKey = new("VAA.EAPI.Web.Common.Http.CookieContainer");

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
