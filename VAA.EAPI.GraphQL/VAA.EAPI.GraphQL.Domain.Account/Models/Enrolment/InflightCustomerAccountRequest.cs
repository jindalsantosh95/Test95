namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class InflightCustomerAccountRequest : AccountRequestContext
    {
        public InflightEnrolCustomerAccountDetails? CustomerDetails { get; set; }
        public string? ChannelUserId { get; set; }
        public string? EnrolmentToken { get; set; }
    }
}
