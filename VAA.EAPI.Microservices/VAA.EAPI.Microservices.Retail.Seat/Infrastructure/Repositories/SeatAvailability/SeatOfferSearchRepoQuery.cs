using MediatR;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class SeatOfferSearchRepoQuery : IRequest<SeatAvailabilityResponse>
    {
        public string SeatOfferID { get; set; }

        public SeatOfferSearchRepoQuery(string seatOfferID)
        {
            SeatOfferID = seatOfferID;
        }
    }
}
