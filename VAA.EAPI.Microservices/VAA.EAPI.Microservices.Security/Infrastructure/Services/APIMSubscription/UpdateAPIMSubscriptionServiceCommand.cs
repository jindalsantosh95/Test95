using MediatR;
using System.Collections.Generic;
using System.Net;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionRequest;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription
{
    public class UpdateAPIMSubscriptionServiceCommand : IRequest<KeyValuePair<HttpStatusCode, string>>
    {
        public APIMSubscriptionUpdateRequest Request { get; set; }
        public string Uri { get; set; }

        public UpdateAPIMSubscriptionServiceCommand(APIMSubscriptionUpdateRequest request, string uri)
        {
            Request = request;
            Uri = uri;
        }
    }
}
