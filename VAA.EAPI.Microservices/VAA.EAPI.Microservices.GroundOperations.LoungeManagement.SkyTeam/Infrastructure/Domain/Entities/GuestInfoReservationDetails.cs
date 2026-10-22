using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class GuestInfoReservationDetails
    {
        [JsonProperty("segments")]
        public Segment[] Segments { get; set; }
    }
}
