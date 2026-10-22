using MediatR;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common
{
    public class CancelReshopOfferSearchRepoQuery : IRequest<CancelReshopResponse>
    {
        public string OfferId { get; set; }

        public CancelReshopOfferSearchRepoQuery(string offerId)
        {
            OfferId = offerId;
        }
    }
}
