using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability
{
    public class SeatOfferSearchQuery : IRequest<Result<Response<SeatAvailabilityResponse>>>
    {
        public string SeatOfferID { get; set; }

        public SeatOfferSearchQuery(string seatOfferID)
        {
            SeatOfferID = seatOfferID;
        }
    }
}
