using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo
{
    public sealed class PassengerInformationResponse
    {
        [JsonProperty("bookingId")]
        public string BookingId { get; set; }

        [JsonProperty("passengers")]
        public IEnumerable<PassengerResponse> Passengers { get; set; }
    }
}
