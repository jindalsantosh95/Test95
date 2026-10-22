using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FareRules;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta
{
    public interface IDeltaOfferService
    {
        Task<OffersSetList> GetOffers(OfferSearchCriteria offerRequest);
        Task<Result<Response<OfferFareRules>>> GetOfferFareRules(string offerID);
    }
}
