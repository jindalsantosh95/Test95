namespace VAA.EAPI.Core.Xunit.Fixtures
{
    public sealed class HttpClientFactoryFixture : IDisposable
    {
        public Http.HttpClientFactory Current { get; }

        public HttpClientFactoryFixture()
        {
            Current = new Http.HttpClientFactory();
        }

        [Obsolete("Use new mocking functionality now ported for unit tests")]
        public void RegisterRequestResponse(HttpRequestMessage request, HttpResponseMessage response) => Http.HttpClientFactory.RegisterRequestResponse(request, response);

        [Obsolete("Use new mocking functionality now ported for unit tests")]
        public void PrepareHeaders(HttpRequestMessage request, Dictionary<string, string>? headers = default, string bearerToken = "", bool preventFollow202Location = false) => Http.HttpClientFactory.PrepareHeaders(request, headers, bearerToken, preventFollow202Location);

        public void Dispose()
        {
        }
    }
}