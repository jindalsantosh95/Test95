using MediatR;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common
{
    public class OfferSearchRepoQuery : IRequest<OfferResponse>
    {
        public string OfferId { get; set; }

        public OfferSearchRepoQuery(string offerId)
        {
            OfferId = offerId;
        }
    }
}
