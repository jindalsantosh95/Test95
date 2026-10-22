using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRS;

namespace VAA.EAPI.NDC.Offer.Application.Query.ServiceList
{
    public class ServiceListQuery : IRequest<Result<Response<IATA_ServiceListRS>>>
    {
        public IATA_ServiceListRQ ServiceListRQ { get; set; }
        public string SubscriptionKey { get; set; }

        public ServiceListQuery(IATA_ServiceListRQ serviceListRQ, string subscriptionKey)
        {
            ServiceListRQ = serviceListRQ;
            SubscriptionKey = subscriptionKey;
        }
    }
}