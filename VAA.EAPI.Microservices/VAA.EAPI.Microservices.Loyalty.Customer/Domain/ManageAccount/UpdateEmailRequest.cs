namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class UpdateEmailRequest
    {
        public string MemberNumber { get; set; }
        public string NewEmailAddress { get; set; }
    }
}
