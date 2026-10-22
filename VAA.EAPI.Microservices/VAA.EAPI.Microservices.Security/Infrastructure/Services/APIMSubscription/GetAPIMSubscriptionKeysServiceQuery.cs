using MediatR;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription
{
    public class GetAPIMSubscriptionKeysServiceQuery : IRequest<KeyValuePair<HttpStatusCode, string>>
    {
        public GetAPIMSubscriptionKeysServiceQuery(string uri, HttpMethod verb)
        {
            Uri = uri;
            Verb = verb;
        }
        public string Uri { get; set; }
        public HttpMethod Verb { get; set; }

    }
}
