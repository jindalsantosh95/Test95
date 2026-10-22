using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class PassengerInfo
    {
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("reservationDetails")]
        public PassengerInfoReservationDetails ReservationDetails { get; set; }

        [JsonProperty("membership")]
        public Membership Membership { get; set; }
    }
}
