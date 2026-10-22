namespace VAA.EAPI.Automation.OCI.Models.GraphQL.PaymentEligibilityCheck
{
    public class Request
    {
        public string checkInId { get; set; }
        public string failureRedirectUrl { get; set; }
        public string successRedirectUrl { get; set; }
    }
}