using HotChocolate;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatFulfilmentRequest")]
    public class SeatFulfilmentRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public IEnumerable<SelectedSeatInfo> SelectedSeatInfo { get; set; }
        public IEnumerable<Payment.PaymentDetails> PaymentDetails { get; set; }
        public string ChannelId { get; set; }
        public IEnumerable<SeatTransactionId> SeatTransactionIds { get; set; }

        public static explicit operator SeatFulfilmentRequest((Basket basket, IEnumerable<BasketMS.FlightPassengerSeatMap> seatAdditionData, IEnumerable<SeatResponse> seatMaps, PaymentTransactionDetails paymentDetails) source)
        {
            var (basket, seatAdditionData, seatMaps, paymentDetails) = source;
            return new SeatFulfilmentRequest
            {
                Pos = basket.BookingDetails.Pos,
                BookingReferences = new BookingReference
                {
                    Rloc = basket.BookingDetails.PnrLocator
                },
                SelectedSeatInfo = seatAdditionData.Select(seat =>
                {
                    var seatOfferMap = seatMaps.FirstOrDefault(sm => sm.SeatOfferId == seat.SeatOfferId);
                    var passenger = basket.BookingDetails.Passenger.FirstOrDefault(p => p.Id == seat.PassengerId);

                    // Get Eligibility object from seatOfferMap on the basis of seat.SelectedSeatOfferId
                    var eligibility = seatOfferMap?.FlightSegmentSeatMap
                        .SelectMany(f => f.Seatmap)
                        .SelectMany(r => r.SeatRows)
                        .SelectMany(c => c.SeatColumns)
                        .FirstOrDefault(s => s.SeatIdentifer == seat.SeatNumber)?.Eligibility;

                    var selectedOffer = eligibility?.FirstOrDefault(e => e.CustomerId.Contains(passenger.CustomerId) && e.CatalogueRefId == seat.SelectedSeatOfferId);
                    var selectedCatalogue = seatOfferMap?.FlightSegmentSeatMap.First().Catalogue?.Where(c => c.CatalogueId == seat.SelectedSeatOfferId).FirstOrDefault();

                    return new SelectedSeatInfo
                    {
                        AssociationId = passenger.CustomerId.Split('-').First(),
                        FlightIdRef = selectedCatalogue.FlightIdRef,
                        OfferId = seat.SelectedSeatOfferId.Split('|').First(),
                        OfferItemNumber = selectedOffer.OfferItemId.First().OfferItemId,
                        SeatNumber = seat.SeatNumber,
                        Amount = seat.Price.Amount > 0m ? (CurrencyAmount)(basket, seat.SeatNumber) : null,
                    };

                }),
                ChannelId = Constants.Application.SeatMSDLChannelId,
                SeatTransactionIds = seatAdditionData.Select(seat => new SeatTransactionId
                {
                    SegmentId = @"0" + seat.SectorId,
                    TransactionId = AsyncContext.Current.CorrelationId.ToString()
                }),
                PaymentDetails = paymentDetails?.PaymentDetails?.Select(pd => (Payment.PaymentDetails)(pd, paymentDetails.PaymentReferenceId, basket))
            };
        }


    }
}