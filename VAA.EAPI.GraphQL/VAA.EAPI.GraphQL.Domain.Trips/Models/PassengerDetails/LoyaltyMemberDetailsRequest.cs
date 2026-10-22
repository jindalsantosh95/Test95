namespace VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails
{
    public class LoyaltyMemberDetailsRequest
    {
        public string TripReservationId { get; set; }
        public string PassengerId { get; set; }
        public string LoyaltyProgramCode { get; set; }
        public string LoyaltyMemberId { get; set; }
    }
}
