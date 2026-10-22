using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS
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
            return Json.Deserialize<TripsDetailsRequest>(Json.Serialize(source));
        }
    }
}