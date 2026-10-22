using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class LoungeEligibilityResponse
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("trackId")]
        public string TrackId { get; set; }

        [JsonProperty("confirmationDetails")]
        public ConfirmationDetails ConfirmationDetails { get; set; }

        [JsonProperty("eligibilityCode")]
        public string EligibilityCode { get; set; }

        [JsonProperty("eligibilityReasons")]
        public EligibilityReason[] EligibilityReasons { get; set; }

        [JsonProperty("numberOfGuests")]
        public long NumberOfGuests { get; set; }

        [JsonProperty("passenger")]
        public Passenger Passenger { get; set; }

        [JsonProperty("guests")]
        public Guest[] Guests { get; set; }

        [JsonProperty("loyaltyStatus")]
        public LoyaltyStatus LoyaltyStatus { get; set; }
    }
}
