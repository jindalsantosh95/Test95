using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class Passenger
    {
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("reservationDetails")]
        public PassengerReservationDetails ReservationDetails { get; set; }

        [JsonProperty("membership")]
        public Membership Membership { get; set; }
    }
}
