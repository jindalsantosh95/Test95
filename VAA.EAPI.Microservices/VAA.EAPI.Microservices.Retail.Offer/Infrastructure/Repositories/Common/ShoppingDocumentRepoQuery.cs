using MediatR;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common
{
    public class ShoppingDocumentRepoQuery : IRequest<OfferResponse>
    {
        public string ShoppingSelectedOfferId { get; set; }

        public ShoppingDocumentRepoQuery(string shoppingSelectedOfferId)
        {
            ShoppingSelectedOfferId = shoppingSelectedOfferId;
        }
    }
}
