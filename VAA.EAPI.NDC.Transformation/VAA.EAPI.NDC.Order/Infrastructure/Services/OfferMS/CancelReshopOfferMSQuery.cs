using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS
{
    public class CancelReshopOfferMSQuery : IRequest<CancelReshopResponse>
    {
        public string OfferID { get; set; }

        public static explicit operator CancelReshopOfferMSQuery(string OfferID) => new CancelReshopOfferMSQuery { OfferID = OfferID };
    }
}
