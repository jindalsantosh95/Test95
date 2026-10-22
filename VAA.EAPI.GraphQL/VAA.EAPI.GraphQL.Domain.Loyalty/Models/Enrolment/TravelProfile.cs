namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class TravelProfile
    {
        public enum ReasonsForTravel
        {
            Business,
            Leisure,
            Both
        }
        public ReasonsForTravel? ReasonForTravel { get; set; }
    }
}
