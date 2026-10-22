using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Common.Exceptions
{
    public sealed class HttpResponseException : Exception
    {
        //
        // Summary:
        //     Gets the HTTP response to return to the client.
        //
        // Returns:
        //     The System.Net.Http.HttpResponseMessage that represents the HTTP response.
        public HttpResponseMessage Response { get; private set; }

        //
        // Summary:
        //     Initializes a new instance of the System.Web.Http.HttpResponseException class.
        //
        //
        // Parameters:
        //   statusCode:
        //     The status code of the response.
        public HttpResponseException(HttpStatusCode statusCode) : this(new HttpResponseMessage(statusCode)) { }

        //
        // Summary:
        //     Initializes a new instance of the System.Web.Http.HttpResponseException class.
        //
        //
        // Parameters:
        //   response:
        //     The HTTP response to return to the client.
        public HttpResponseException(HttpResponseMessage response) : base("Processing of the HTTP request resulted in an exception. Please see the HTTP response returned by the 'Response' property of this exception for details.")
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public async Task<string> ReadResponseContentAsStringAsync() => await ReadResponseContentAsStringAsync(CancellationToken.None);
        public async Task<string> ReadResponseContentAsStringAsync(CancellationToken cancellationToken) => await Response?.Content.ReadAsStringAsync(cancellationToken) ?? string.Empty;
    }
}
