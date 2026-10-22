using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS
{
    public class PaymentRequestMs
    {
        public PaymentPos Pos { get; set; }
        public Payment PaymentAmt { get; set; }
        public LoyaltyMember LoyaltyMemberDetails { get; set; }
        public IEnumerable<CustomerInfo> CustomerDetails { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }     
        public AdditionalConfigParams AdditionalConfigParams { get; set; }
        public IEnumerable<Seat> SeatDetails { get; set; }
        public string TripType { get; set; }

        public static explicit operator PaymentRequestMs((Basket basket, PaymentRequest request, IEnumerable<SeatResponse> seatResponses, IEnumerable<CabinRefData> cabinRefData, IEnumerable<CurrencyNumberRefData> currencyNumbers) source)
        {
            var (basket, request, seatResponses, cabinRefData, currencyNumbers) = source;

            var booking = basket.BookingDetails;
            var requestedSeatsPax = basket.FlightPassengerSeatMap.Where(f => !f.SeatNumber.IsNullOrEmpty()).Select(x => x.PassengerId).Distinct(); 
            var basketPax = basket.BookingDetails.Passenger;
            var requestedSeatsSegments = basket.FlightPassengerSeatMap.Where(f => !f.SeatNumber.IsNullOrEmpty()).Select(x => x.SectorId).Distinct();

            return new PaymentRequestMs()
            {
                Pos = (PaymentPos)(booking.Pos, request.IpAddress, currencyNumbers),
                PaymentAmt = new() { CurrencyEquivalentPrice = new() { Amount = new() { Currency = booking.Pos.CurrencyCode } } },
                CustomerDetails = basketPax.Select(p =>
                {
                    var customer = new CustomerInfo()
                    {
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        Ptc = p.Type,
                        Contacts = new() { Countrycode = "", Email = basket?.ContactDetails?.EmailAddress, Phone = "", PhoneAreaNumber = ""} 
                    };
                    return customer;
                }),
                FlightSegments = requestedSeatsSegments.Select(s =>
                {
                    var slice = booking.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == s));
                    var requestedSegment = slice?.Segments.FirstOrDefault(seg => seg.Id == s);

                    return (Segment)requestedSegment;
                }),
                SeatDetails = basket.FlightPassengerSeatMap.Where(f => !f.SeatNumber.IsNullOrEmpty()).Select(seat =>
                {
                    var seatOfferMap = seatResponses.FirstOrDefault(sm => sm.SeatOfferId == seat.SeatOfferId);
                    var passenger = basket.BookingDetails.Passenger.FirstOrDefault(p => p.Id == seat.PassengerId);

                    var eligibility = seatOfferMap?.FlightSegmentSeatMap
                        .SelectMany(f => f.Seatmap)
                        .SelectMany(r => r.SeatRows)
                        .SelectMany(c => c.SeatColumns)
                        .FirstOrDefault(s => s.SeatIdentifer == seat.SeatNumber)?.Eligibility;
                    var selectedOffer = eligibility?.FirstOrDefault(e => e.CustomerId.Contains(passenger.CustomerId) && e.CatalogueRefId == seat.SelectedSeatOfferId);
                    var selectedCatalogue = seatOfferMap?.FlightSegmentSeatMap.First().Catalogue?.Where(c => c.CatalogueId == seat.SelectedSeatOfferId).FirstOrDefault();

                    var slice = booking.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == seat.SectorId));

                    var seatItem = new Seat()
                    {
                        TripId = slice.Id,
                        TripSeq = slice.Id,
                        PassengerSequence = (requestedSeatsPax.ToList().IndexOf(seat.PassengerId) + 1).ToString(),
                        PendingSeatNumber = seat.SeatNumber,
                        PendingSeatPriceAmount = selectedCatalogue.SeatPrice.MonetaryAmount,
                        PendingSeatCurrencyCode = selectedCatalogue.SeatPrice.BaseCurrency,
                        PendingSeatDecimalPrecisionCount = selectedCatalogue.SeatPrice.DecimalPrecisionCount.ToString(),
                        SeatOfferDetails = new()
                        {
                            DeltaSeatOfferId = seat.SelectedSeatOfferId.Split('|').First(),
                            BrandId = selectedCatalogue.BrandId,
                            BrandDescription = cabinRefData.First(c => c.BrandID == selectedCatalogue.BrandId)?.BrandName,
                            ProductDescription = selectedCatalogue.ProductName
                        }
                    };

                    return seatItem;
                }),
                AdditionalConfigParams = new()
                {
                    FailureRedirectUrl = request.PaymentFailureLink,
                    MultiProductFlag = true,
                    SuccessRedirectUrl = request.PaymentSuccessLink,
                    SessionID = AsyncContext.Current.CorrelationId.ToString(),
                    IsInsuranceAdded = false
                },
                TripType = "MYB"
            };
        }
    }
}