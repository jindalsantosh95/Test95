namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class AccountContactDetails
    {
        public List<AccountEmailDetail>? EmailDetails { get; set; }
        public List<AccountPhoneDetail>? PhoneDetails { get; set; }
    }
}
