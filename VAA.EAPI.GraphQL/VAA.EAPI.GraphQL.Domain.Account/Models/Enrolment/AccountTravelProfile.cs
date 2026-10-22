namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class AccountTravelProfile
    {
        public enum AccountReasonsForTravel
        {
            Business,
            Leisure,
            Both
        }
        public AccountReasonsForTravel? ReasonForTravel { get; set; }
    }
}
