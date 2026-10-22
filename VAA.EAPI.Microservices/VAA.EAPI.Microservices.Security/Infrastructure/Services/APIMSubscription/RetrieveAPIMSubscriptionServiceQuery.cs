using MediatR;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription
{
    public class RetrieveAPIMSubscriptionServiceQuery : IRequest<KeyValuePair<HttpStatusCode, string>>
    {
        public RetrieveAPIMSubscriptionServiceQuery(string uri, HttpMethod verb)
        {
            Uri = uri;
            Verb = verb;

        }
        public string Uri { get; set; }
        public HttpMethod Verb { get; set; }


    }
}
