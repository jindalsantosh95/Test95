using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class GuestInfoReservationDetails
    {
        [JsonProperty("segments")]
        public Segment[] Segments { get; set; }
    }
}
