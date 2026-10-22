using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Security.Claims;
using VAA.EAPI.Core.Functions;

namespace VAA.EAPI.Core.Functions
{
    public sealed class TestHttpRequestData : HttpRequestData
    {
        private readonly Uri _url;
        private readonly MemoryStream _body = new();
        private readonly string _method;
        private readonly HttpHeadersCollection _headers = [];
        private readonly List<IHttpCookie> _cookies = [];
        private readonly List<ClaimsIdentity> _identities = [];

        public TestHttpRequestData(FunctionContext functionContext, Uri url, Stream? body = null, string method = "GET", Dictionary<string, string>? headers = null, IEnumerable<IHttpCookie>? cookies = null, IEnumerable<ClaimsIdentity>? identities = null) : base(functionContext)
        {
            _url = url;

            body?.CopyTo(_body);

            _body.Position = 0;

            _method = method;

            if (headers is not null)
                foreach (var header in headers)
                    _headers.Add(header.Key, header.Value);

            if (cookies is not null)
                _cookies.AddRange(cookies);

            if (identities is not null)
                _identities.AddRange(identities);
        }

        public override Uri Url => _url;
        public override Stream Body => _body;
        public override HttpHeadersCollection Headers => _headers;
        public override IReadOnlyCollection<IHttpCookie> Cookies => _cookies;
        public override IEnumerable<ClaimsIdentity> Identities => _identities;
        public override string Method => _method;
        public override HttpResponseData CreateResponse() => new TestHttpResponseData(FunctionContext);
    }
}
