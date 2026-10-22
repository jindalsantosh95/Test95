using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public sealed class HolidayRequest
    {
        public string BookingReference { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Surname { get; set; }

        public static explicit operator HolidayRequest(TripsDetailsRequest source)
        {
            if (source == null) return null;

            ArgumentNullException.ThrowIfNullOrWhiteSpace(source.BookingReference);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(source.Surname);
            ArgumentNullException.ThrowIfNull(source.DateOfBirth);

            return new()
            {
                BookingReference = source.BookingReference,
                DateOfBirth = source.DateOfBirth.GetValueOrDefault(),
                Surname = source.Surname
            };
        }
    }
}