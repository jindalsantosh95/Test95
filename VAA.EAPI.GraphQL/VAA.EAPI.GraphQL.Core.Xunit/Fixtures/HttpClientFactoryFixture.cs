using VAA.EAPI.GraphQL.Core.Xunit.Http;

namespace VAA.EAPI.GraphQL.Core.Xunit.Fixtures
{
    
    [Obsolete("This class is obsolete. Register VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory directly in your ServiceCollection.")]
    public sealed class HttpClientFactoryFixture : IDisposable
    {
        public HttpClientFactory Current { get; }

        public HttpClientFactoryFixture()
        {
            Current = new HttpClientFactory();
        }

        public void RegisterRequestResponse(HttpRequestMessage request, HttpResponseMessage response) => HttpClientFactory.RegisterRequestResponse(request, response);

        public void PrepareHeaders(HttpRequestMessage request, Dictionary<string, string>? headers = default, string bearerToken = "", bool preventFollow202Location = false) => HttpClientFactory.PrepareHeaders(request, headers, bearerToken, preventFollow202Location);

        public void Dispose()
        {
        }
    }
}