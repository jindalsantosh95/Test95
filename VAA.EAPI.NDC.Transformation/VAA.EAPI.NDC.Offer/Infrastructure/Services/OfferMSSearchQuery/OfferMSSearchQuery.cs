using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSSearchQuery
{
    public class OfferMSSearchQuery : IRequest<OfferResponse>
    {
        public string OfferID { get; set; }

        public OfferMSSearchQuery(string offerID)
        {
            OfferID = offerID;
        }
    }
}
