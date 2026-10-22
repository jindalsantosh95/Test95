using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Inflight.ConnPulse.Helper
{
    public static class HttpRequestFallbackHelper
    {
        public static async Task<HttpResponseMessage> SendWithHeadFallbackToGetAsync(IHttpConnector httpConnector, HttpRequestMessage headRequest, Uri uri, CancellationToken cancellationToken)
        {
            var headResponse = await httpConnector.SendAsync(headRequest, cancellationToken).ConfigureAwait(false);

            if (headResponse.StatusCode is not (System.Net.HttpStatusCode.MethodNotAllowed or System.Net.HttpStatusCode.NotFound))
            {
                return headResponse;
            }

            var requestHeaders = headRequest.Headers.ToArray();
            headResponse.Dispose();

            using var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            foreach (var header in requestHeaders)
            {
                getRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return await httpConnector.SendAsync(getRequest, cancellationToken).ConfigureAwait(false);
        }
    }
}
