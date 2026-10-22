using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS
{
    public class SeatMSQuery : IRequest<SeatRS>
    {
        public string OfferID { get; set; }

        public static explicit operator SeatMSQuery(string OfferID) => new SeatMSQuery { OfferID = OfferID };
    }
}
