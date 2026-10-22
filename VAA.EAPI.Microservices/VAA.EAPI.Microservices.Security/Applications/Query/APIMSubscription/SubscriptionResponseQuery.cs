using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
namespace VAA.EAPI.Microservices.Security.Applications.Query.APIMSubscription
{
    public class SubscriptionResponseQuery : IRequest<Result<Response<SubscriptionResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}
