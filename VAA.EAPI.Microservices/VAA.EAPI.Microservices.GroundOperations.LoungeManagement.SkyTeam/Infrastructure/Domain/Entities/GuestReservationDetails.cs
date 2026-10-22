using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class GuestReservationDetails
    {
        [JsonProperty("segments")]
        public Segment[] Segments { get; set; }
    }
}
