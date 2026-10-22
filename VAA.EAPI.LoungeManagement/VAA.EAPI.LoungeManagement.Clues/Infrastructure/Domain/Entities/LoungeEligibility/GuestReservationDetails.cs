using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class GuestReservationDetails
    {
        [JsonProperty("segments")]
        public Segment[] Segments { get; set; }
    }
}
