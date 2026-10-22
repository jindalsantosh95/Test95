using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class PassengerInfoReservationDetails
    {
        [JsonProperty("eticket")]
        public string Eticket { get; set; }

        [JsonProperty("bookingReference")]
        public string BookingReference { get; set; }

        [JsonProperty("segments")]
        public Segment[] Segments { get; set; }
    }
}
