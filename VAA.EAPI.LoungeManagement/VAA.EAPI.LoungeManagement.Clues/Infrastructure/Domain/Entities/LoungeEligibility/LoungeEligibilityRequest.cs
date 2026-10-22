using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class LoungeEligibilityRequest
    {
        [JsonProperty("trackId")]
        public string TrackId { get; set; }

        [JsonProperty("loungeInfo")]
        public LoungeInfo LoungeInfo { get; set; }

        [JsonProperty("passengerInfo")]
        public PassengerInfo PassengerInfo { get; set; }

        [JsonProperty("numberOfGuests")]
        public long NumberOfGuests { get; set; }

        [JsonProperty("guestInfo")]
        public GuestInfo[] GuestInfo { get; set; }
    }
}
