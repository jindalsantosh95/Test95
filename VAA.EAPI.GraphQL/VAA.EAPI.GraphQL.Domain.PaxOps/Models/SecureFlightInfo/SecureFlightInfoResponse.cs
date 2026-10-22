using HotChocolate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo;
using VAA.EAPI.GraphQL.Common.Extensions;
using Trips = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.SecureFlightInfo
{
    public sealed class SecureFlightInfoResponse
    {
        public string SecureFlightReservationId { get; set; }
        public SecureFlightPassengerDetails PassengerDetails { get; set; }

        [GraphQLDescription("Whether any booking segment has the United States as its origin or destination country. Only populated for the secure flight information query.")]
        public bool? ShowRedressNumber { get; set; }

        [GraphQLDescription("Whether any booking segment has Canada as its origin or destination country. Only populated for the secure flight information query.")]
        public bool? ShowCanadianTravelNumber { get; set; }

        public static explicit operator SecureFlightInfoResponse((string SecureFlightReservationId, PassengerResponse Passenger) source) => new()
        {
            SecureFlightReservationId = source.SecureFlightReservationId,
            PassengerDetails = new SecureFlightPassengerDetails
            {
                PassengerId = source.Passenger.PassengerResourceId,
                PassengerFirstNin = source.Passenger.FirstNameNum,
                PassengerLastNin = source.Passenger.LastNameNum,
                KnownTravellerNumber = GetProgramNumber(source.Passenger, "KNOWN_TRAVELER_NUMBER"),
                CanadianTravelNumber = GetProgramNumber(source.Passenger, "CANADA_TRAVELER_NUMBER"),
                RedressNumber = GetProgramNumber(source.Passenger, "REDRESS_NUMBER")
            }
        };

        public static explicit operator SecureFlightInfoResponse((Trips.Booking Booking, PassengerResponse Passenger) source) => new()
        {
            SecureFlightReservationId = source.Booking.Id,
            PassengerDetails = new SecureFlightPassengerDetails
            {
                PassengerId = source.Passenger.PassengerResourceId,
                PassengerFirstNin = source.Passenger.FirstNameNum,
                PassengerLastNin = source.Passenger.LastNameNum,
                KnownTravellerNumber = GetProgramNumber(source.Passenger, "KNOWN_TRAVELER_NUMBER"),
                CanadianTravelNumber = GetProgramNumber(source.Passenger, "CANADA_TRAVELER_NUMBER"),
                RedressNumber = GetProgramNumber(source.Passenger, "REDRESS_NUMBER")
            },
            ShowRedressNumber = HasSegmentCountry(source.Booking, "US"),
            ShowCanadianTravelNumber = HasSegmentCountry(source.Booking, "CA")
        };

        private static string? GetProgramNumber(PassengerResponse passenger, string program)
        {
            var number = passenger.AdvancePassengerInformation?.TrustedTravellerPrograms?
                .FirstOrDefault(item => item.Program.Is(program))?.Number;

            return number.IsNullOrWhiteSpace() ? null : number;
        }

        private static bool HasSegmentCountry(Trips.Booking booking, string countryCode) =>
            booking.Product?
                .SelectMany(product => product.Slices ?? [])
                .SelectMany(slice => slice.Segments ?? [])
                .Any(segment =>
                    segment.Origin?.CountryCode.Is(countryCode) == true ||
                    segment.Destination?.CountryCode.Is(countryCode) == true) == true;

    }
}
