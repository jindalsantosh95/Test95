namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class CreateCustomerAccountRequest : AccountRequestContext
    {
        public CustomerAccountDetails? CustomerDetails { get; set; }
        public string? ChannelUserId { get; set; }
        public string? EnrolmentToken { get; set; }
    }
}
