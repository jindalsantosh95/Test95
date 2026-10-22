namespace VAA.EAPI.Web.Common.Infrastructure.Interfaces
{
    using System.Net;

    public interface IHttpConnector
    {
        Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers, string bearerToken);
        Task<HttpResponseMessage> DeleteAsync(string uri, Dictionary<string, string> headers, string bearerToken);

        Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, bool noWait = false);
        Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, CookieContainer cookies, bool noWait = false);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CookieContainer cookies, CancellationToken cancellationToken, bool noWait = false);
        Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false);
        Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpRequestMessage httpRequestMessage, CookieContainer cookies, CancellationToken cancellationToken, bool noWait = false);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken);
    }
}
