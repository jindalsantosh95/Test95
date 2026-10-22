using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
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

        public static explicit operator TripsDetailsRequest(RetrieveBookingRequest source)
        {
            if (source == null) return null;

            return new()
            {
                Surname = source.Surname,
                BookingReference = source.BookingReference,
                DateOfBirth = source.DateOfBirth,
                Using = RequestOptionCodeType.HOLIDAYS
            };


        }
    }
}
