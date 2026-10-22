using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS
{
    public class SeatMSOfferQuery :IRequest<SeatDoc>
    {
        public string OfferID { get; set; }

        public static explicit operator SeatMSOfferQuery(string OfferID) => new SeatMSOfferQuery { OfferID = OfferID };
    }
}
