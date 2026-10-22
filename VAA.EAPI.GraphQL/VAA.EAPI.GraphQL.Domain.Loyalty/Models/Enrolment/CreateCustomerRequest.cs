namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class CreateCustomerRequest : RequestContext
    {
        public CustomerDetails? CustomerDetails { get; set; }
        public string? ChannelUserId { get; set; }
    }
}
