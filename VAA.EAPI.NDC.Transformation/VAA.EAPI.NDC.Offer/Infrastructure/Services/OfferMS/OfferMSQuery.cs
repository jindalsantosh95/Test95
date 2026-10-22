using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMS
{
    public class OfferMSQuery : IRequest<OfferResponse>
    {
        public string OfferID { get; set; }

        public static explicit operator OfferMSQuery(string OfferID) => new OfferMSQuery { OfferID = OfferID };
    }
}
