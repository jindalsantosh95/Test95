namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer
{
    public class AccountTravelProfile
    {
        public enum AccountReasonForTravel
        {
            Business,
            Leisure,
            Both
        }
        public AccountReasonForTravel? ReasonForTravel { get; set; }
    }
}
