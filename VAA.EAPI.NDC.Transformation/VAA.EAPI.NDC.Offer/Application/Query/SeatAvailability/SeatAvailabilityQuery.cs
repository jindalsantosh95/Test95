using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;


namespace VAA.EAPI.NDC.Offer.Application.Query.SeatAvailability
{
    public class SeatAvailabilityQuery : IRequest<Result<Response<IATA_SeatAvailabilityRS>>>
    {
        public IATA_SeatAvailabilityRQ SeatAvailabilityRQ { get; set; }
        public string SubscriptionKey { get; set; }

        public SeatAvailabilityQuery(IATA_SeatAvailabilityRQ seatAvailabilityRQ, string subscriptionKey)
        {
            SeatAvailabilityRQ = seatAvailabilityRQ;
            SubscriptionKey = subscriptionKey;

        }
    }
}
