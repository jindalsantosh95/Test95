using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class TripsDetailsRequest
    {
        public RequestOptionCodeType Using { get; set; }
        public string? ConfirmationNum { get; set; }
        public string? EncryptedConfirmationNum { get; set; }
        public string? TicketNum { get; set; }
        public string? CreditCardNum { get; set; }
        public string? LoyaltyNum { get; set; }
        public string? GivenNames { get; set; }
        public string? Surname { get; set; }
        public string? DepartureAirportCode { get; set; }
        public string? BookingReference { get; set; }
        public DateOnly? DateOfBirth { get; set; }

    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestOptionCodeType
    {
        [EnumMember(Value = "PNR")]
        PNR = 1,
        [EnumMember(Value = "TICKET")]
        TICKET = 2,
        [EnumMember(Value = "CREDITCARD")]
        CREDITCARD = 3,
        [EnumMember(Value = "LOYALTYNUM")]
        LOYALTYNUM = 4,
        [EnumMember(Value = "HOLIDAYS")]
        HOLIDAYS = 5

    }
}
