namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class TripsDetailsRequest
    {
        public RequestOptionCodeType Using { get; set; }
        public string ConfirmationNum { get; set; }
        public string EncryptedConfirmationNum { get; set; }
        public string TicketNum { get; set; }
        public string CreditCardNum { get; set; }
        public string LoyaltyNum { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string DepartureAirportCode { get; set; }
        public string BookingReference { get; set; }
        public DateOnly DateOfBirth { get; set; }

        public static explicit operator TripsDetailsRequest(Trips.Models.Trips.Virgin.TripsDetailsRequest source)
        {
            if (source == null) return null;

            ArgumentNullException.ThrowIfNullOrWhiteSpace(source.BookingReference);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(source.Surname);
            ArgumentNullException.ThrowIfNull(source.DateOfBirth);

            return new()
            {
                BookingReference = source.BookingReference,
                Surname = source.Surname,
                DateOfBirth = source.DateOfBirth.GetValueOrDefault(),
                Using = RequestOptionCodeType.HOLIDAYS
            };
        }
    }
}