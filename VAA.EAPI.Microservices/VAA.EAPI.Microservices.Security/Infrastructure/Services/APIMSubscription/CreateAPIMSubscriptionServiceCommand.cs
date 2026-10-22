using MediatR;
using System.Collections.Generic;
using System.Net;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionRequest;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription
{
    public class CreateAPIMSubscriptionServiceCommand : IRequest<KeyValuePair<HttpStatusCode, string>>
    {
        public APIMSubscriptionCreateRequest Request { get; set; }
        public string Uri { get; set; }

        public CreateAPIMSubscriptionServiceCommand(APIMSubscriptionCreateRequest request, string uri)
        {
            Request = request;
            Uri = uri;

        }
    }
}
