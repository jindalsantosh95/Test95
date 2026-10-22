namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class EmailTriggerRequestVacManageAccount
    {
        public string To { get; set; }
        public string Template { get; set; }
        public string FirstName { get; set; }
        public string TierCode { get; set; }
    }
}
