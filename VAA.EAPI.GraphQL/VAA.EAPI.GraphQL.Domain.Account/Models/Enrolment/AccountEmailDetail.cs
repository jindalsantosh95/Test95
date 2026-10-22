namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class AccountEmailDetail
    {
        public enum AccountEmailUsages
        {
            Business,
            Personal
        }
        public AccountEmailUsages? Usage { get; set; }
        public string? EmailId { get; set; }
        public string? PreferredFlag { get; set; }
        public string? Description { get; set; }
        public string? Valid { get; set; }
    }
}
