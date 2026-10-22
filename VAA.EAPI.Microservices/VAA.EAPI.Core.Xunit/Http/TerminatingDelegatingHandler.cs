namespace VAA.EAPI.Core.Xunit.Http
{
    public sealed class TerminatingDelegatingHandler : DelegatingHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => new(System.Net.HttpStatusCode.Gone)
        {
            RequestMessage = request,
            Content = new StringContent("Access to external resources in unit tests is not allowed.", System.Text.Encoding.UTF8, "text/plain")
        };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => new(System.Net.HttpStatusCode.Gone)
        {
            RequestMessage = request,
            Content = new StringContent("Access to external resources in unit tests is not allowed.", System.Text.Encoding.UTF8, "text/plain")
        };
    }
}
