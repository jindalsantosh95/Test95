using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Applications.Query.APIMSubscription
{
    public class SubscriptionRetrieveQuery : IRequest<Result<Response<SubscriptionResponse>>>
    {
        public string SubscriptionId { get; set; }
        public string Channel { get; set; }
        public string Environment { get; set; }
    }
}

