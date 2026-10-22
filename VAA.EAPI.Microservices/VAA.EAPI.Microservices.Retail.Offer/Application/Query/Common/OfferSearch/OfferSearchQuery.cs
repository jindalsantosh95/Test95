using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Common.OfferSearch
{
    public class OfferSearchQuery : IRequest<Result<Response<OfferResponse>>>
    {
        public string OfferID { get; set; }

        public OfferSearchQuery(string offerID)
        {
            OfferID = offerID;
        }
    }
}
