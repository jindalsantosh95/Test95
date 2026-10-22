using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS
{
    public class OfferMSQuery : IRequest<OfferResponse>
    {
        public string OfferID { get; set; }

        public static explicit operator OfferMSQuery(string OfferID) => new OfferMSQuery { OfferID = OfferID };
    }
}
