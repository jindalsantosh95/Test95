using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IHttpConnector
    {
        // Typed overloads consume and deserialize the complete body. Raw overloads return
        // successful responses after headers; callers must dispose the response and supply
        // cancellation/deadlines when reading its body. noWait overloads retain error mapping
        // and consume error bodies. The raw two/three-argument overloads leave status handling
        // to the caller. Middleware caching/mocking must not buffer a streaming route.
        Task<APIResponse<T>> GetAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken);
        Task<APIResponse<T>> DeleteAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken);

        Task<APIResponse<T>> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, bool noWait = false);
        Task<APIResponse<T>> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, CookieContainer cookies, bool noWait = false);
        Task<APIResponse<T>> SendAsync<T>(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false);
        Task<APIResponse<T>> SendAsync<T>(HttpRequestMessage httpRequestMessage, CookieContainer cookies, CancellationToken cancellationToken, bool noWait = false);

        Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, bool noWait = false);
        Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, CookieContainer cookies, bool noWait = false);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CookieContainer cookies, CancellationToken cancellationToken, bool noWait = false);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken);
    }
}
