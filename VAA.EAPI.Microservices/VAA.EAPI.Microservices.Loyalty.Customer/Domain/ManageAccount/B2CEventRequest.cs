namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class B2CEventRequest
    {
        public string MemberNumber { get; set; }
        public string CurrentEmailId { get; set; }
        public string NewEmailId { get; set; }
    }
}
