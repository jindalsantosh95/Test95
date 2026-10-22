using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Common.CancelReshopOfferSearch
{
    public class CancelReshopOfferSearchQuery : IRequest<Result<Response<CancelReshopResponse>>>
    {
        public string OfferID { get; set; }

        public CancelReshopOfferSearchQuery(string offerID)
        {
            OfferID = offerID;
        }
    }
}
