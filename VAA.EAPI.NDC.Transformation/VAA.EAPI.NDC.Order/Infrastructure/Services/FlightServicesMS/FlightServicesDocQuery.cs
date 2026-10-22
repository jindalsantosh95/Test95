using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.FlightServiceMS
{
    public class FlightServiceOfferDocQuery : IRequest<FlightOfferServiceRS>
    {
        public string OfferID { get; set; }

        public static explicit operator FlightServiceOfferDocQuery(string OfferID) => new FlightServiceOfferDocQuery { OfferID = OfferID };
    }
}
