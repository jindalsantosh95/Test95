using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Interfaces
{
    /// <summary>
    /// Sends outbound HTTP requests. Callers of methods returning <see cref="HttpResponseMessage"/>
    /// must consume the response content or dispose the response to release the connection.
    /// </summary>
    public interface IHttpConnector
    {
        Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers, string bearerToken = null);
        Task<T> GetAsync<T>(string uri, Dictionary<string, string> headers = null, string bearerToken = null);
        Task<HttpResponseMessage> PostAsync(string uri, HttpContent content, Dictionary<string, string> headers = null, string bearerToken = null);
        Task<T> PostAsync<T>(string uri, HttpContent content, Dictionary<string, string> headers = null, string bearerToken = null);
        Task<T> DeleteAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken = null);
        Task<T> PatchAsync<T>(string uri, HttpContent content, Dictionary<string, string> headers, string bearerToken = null);
        Task<T> PutAsync<T>(string uri, HttpContent content, Dictionary<string, string> headers, string bearerToken = null);

        Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, string content, bool preventFollow202Location = false);
        Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, string content, CookieContainer cookies, bool preventFollow202Location = false);
        Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, bool preventFollow202Location = false);
        Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, CancellationToken cancellationToken, bool preventFollow202Location = false);
        Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, CookieContainer cookies, bool preventFollow202Location = false);
        Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod verb, HttpContent content, bool preventFollow202Location = false);
        Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod verb, HttpContent content, CookieContainer cookies, bool preventFollow202Location = false);
        Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod verb, string content, bool preventFollow202Location = false);
        Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod verb, string content, CookieContainer cookies, bool preventFollow202Location = false);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken);
        Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken);
        Task<T> SendAsync<T>(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken);
    }
}
