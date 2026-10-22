using HotChocolate;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsBooking")]
    public class Booking : Holidays.Booking
    {
        public string? Status { get; set; }
        public string? Id { get; set; }
        public Pos? Pos { get; set; }
        public string? ReservationType { get; set; }
        public IEnumerable<Product>? Product { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }
        public BookingResponseDetails? ResponseStatus { get; set; }
        public TripDetails? TripSearchRequest { get; set; }
        public bool? IsAccountLinkedPNR { get; set; }
        public string? TripResourceId { get; set; }
        public IEnumerable<AdvisoryInformation>? ImportantInformation { get; set; }
        public DocumentInformation? DocumentDetails { get; set; }
        public bool PotentialUpgradeAvailable => Product?
            .Where(product => product is { SupportedFlight: true })
            .SelectMany(product => product.Slices ?? [])
            .SelectMany(slice => slice.Segments ?? [])
            .Any(segment => segment.PotentialUpgradeAvailable) == true;

        public static explicit operator Booking((Infrastructure.Models.TripMS.TripsDetailsResponse tripsDetailsResponse, IEnumerable<CabinRefData> cabinRefData, string? bookingId, IEnumerable<AirportRefData> airportRefData, Infrastructure.Models.TripMS.TripsDetailsRequest tripsDetailsRequest, IEnumerable<TripDetails> tripDetails, IEnumerable<AirlineRefData> airlineRefData) source)
            => (Booking)(source.tripsDetailsResponse, source.cabinRefData, source.bookingId, source.airportRefData, source.tripsDetailsRequest, source.tripDetails, source.airlineRefData, null);

        public static explicit operator Booking((Infrastructure.Models.TripMS.TripsDetailsResponse tripsDetailsResponse, IEnumerable<CabinRefData> cabinRefData, string? bookingId, IEnumerable<AirportRefData> airportRefData, Infrastructure.Models.TripMS.TripsDetailsRequest tripsDetailsRequest, IEnumerable<TripDetails> tripDetails, IEnumerable<AirlineRefData> airlineRefData, Infrastructure.Models.CheckInMS.TravelRequirementsResponse travelRequirements) source)
        {
            var (tripsDetailsResponse, cabinRefData, bookingId, airportRefData, tripsDetailsRequest, tripDetails, airlineRefData, travelRequirements) = source;

            if (tripsDetailsResponse.Errors == null)
            {
                var booking = (Booking)(tripsDetailsResponse.Booking, cabinRefData, bookingId, airportRefData, airlineRefData, travelRequirements);

                booking.TripSearchRequest = (TripDetails)tripsDetailsRequest;
                booking.ResponseStatus = (BookingResponseDetails)tripsDetailsResponse;
                booking.IsAccountLinkedPNR = tripDetails.Any(tr => tr.PnrLocator.Is(tripsDetailsRequest?.ConfirmationNum) && !tr.FlyingClubNumber.IsNullOrWhiteSpace());

                return booking;
            }
            else
            {
                return new Booking()
                {
                    TripSearchRequest = (TripDetails)tripsDetailsRequest,
                    ResponseStatus = (BookingResponseDetails)tripsDetailsResponse
                };
            }
        }

        public static explicit operator Booking((Infrastructure.Models.TripMS.Booking booking, IEnumerable<CabinRefData> cabinRefData, string bookingId, IEnumerable<AirportRefData> airportRefData, IEnumerable<AirlineRefData> airlineRefData) source)
            => (Booking)(source.booking, source.cabinRefData, source.bookingId, source.airportRefData, source.airlineRefData, null);

        public static explicit operator Booking((Infrastructure.Models.TripMS.Booking booking, IEnumerable<CabinRefData> cabinRefData, string bookingId, IEnumerable<AirportRefData> airportRefData, IEnumerable<AirlineRefData> airlineRefData, Infrastructure.Models.CheckInMS.TravelRequirementsResponse travelRequirements) source)
        {
            var (booking, cabinRefData, bookingId, airportRefData, airlineRefData, travelRequirements) = source;
            var bookingSlices = booking.Product?.SelectMany(product => product.Slices ?? []);

            return new Booking()
            {
                Status = booking.Status,
                Id = bookingId,
                Passengers = booking.Passengers?.Select(x => (Passenger)(x, cabinRefData, travelRequirements, bookingSlices)),
                Pos = (Pos)(booking, airportRefData),
                ReservationType = booking.ReservationType,
                Product = booking.Product?.Select(x => (Product)(x, cabinRefData, airlineRefData)),
            };
        }

        public static explicit operator Booking((Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsResponse response, IEnumerable<AirportRefData> refAirportData, Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsRequest tripDetailsRequest, IEnumerable<PaymentMethod> refPaymentMethodData, string? tripResourceId) source)
            => (Booking)(source.response, source.refAirportData, source.tripDetailsRequest, source.refPaymentMethodData, source.tripResourceId, Enumerable.Empty<AirlineRefData>());

        public static explicit operator Booking((Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsResponse response, IEnumerable<AirportRefData> refAirportData, Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsRequest tripDetailsRequest, IEnumerable<PaymentMethod> refPaymentMethodData, string? tripResourceId, IEnumerable<AirlineRefData> airlineRefData) source)
        {
            var (response, refAirportData, tripRequest, refPaymentMethodData, tripResourceId, airlineRefData) = source;

            if (response == null)
                return new Booking()
                {
                    TripSearchRequest = (TripDetails)tripRequest,
                };

            var booking = response.Booking;

            if (response.Booking == null)
            {
                return new Booking()
                {
                    TripSearchRequest = (TripDetails)tripRequest,
                    ResponseStatus = (BookingResponseDetails)response,
                };
            }
            else
            {
                return new Booking()
                {
                    Status = booking.Status,
                    Id = booking.Id,
                    Product = booking.HolidayProducts?.Select(x => (Product)(x, refAirportData, airlineRefData)),
                    Destination = (Holidays.Location)booking.HolidayPackageDetails?.Destination,
                    TravelDates = (TravelDatesDetails)booking.Summary,
                    Passengers = booking.Travellers?.Select(x => (Passenger)x),
                    Payment = (BookingPayment)(booking.Payment, refPaymentMethodData, booking.HolidayPackageDetails),
                    TripSearchRequest = (TripDetails)tripRequest,
                    ResponseStatus = (BookingResponseDetails)response,
                    TripResourceId = tripResourceId,
                    ImportantInformation = booking.AdvisoryNotes?.Select(adv => (AdvisoryInformation)adv),
                    DocumentDetails = (DocumentInformation)booking.Summary?.TravelDates,
                    HolidayBookingSummary = (HolidayBookingSummary)booking.BookingSummary
                };
            }
        }
    }
}
