using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips
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

    }
}