using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace VAA.EAPI.GraphQL.Core.Functions
{
    public sealed class TestHttpResponseData : HttpResponseData
    {
        private sealed class HttpCookiesCollection : HttpCookies
        {
            private readonly List<IHttpCookie> _cookies = [];

            public override void Append(string name, string value) => Append(new HttpCookie(name, value));

            public override void Append(IHttpCookie cookie) => _cookies.Add(cookie);

            public override IHttpCookie CreateNew()
            {
                var httpCookie = new HttpCookie(Guid.NewGuid().ToString("N"), string.Empty);

                Append(httpCookie);

                return httpCookie;
            }
        }

        public TestHttpResponseData(FunctionContext functionContext) : base(functionContext) { }

        public override HttpStatusCode StatusCode { get; set; }
        public override HttpHeadersCollection Headers { get; set; } = [];
        public override Stream Body { get; set; } = new MemoryStream();
        public override HttpCookies Cookies { get; } = new HttpCookiesCollection();
    }
}
