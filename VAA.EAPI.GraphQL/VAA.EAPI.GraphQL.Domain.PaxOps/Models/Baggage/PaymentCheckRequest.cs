namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class PaymentCheckRequest
    {
        public string CheckInId { get; set; }
        public string FailureRedirectUrl { get; set; }
        public string SuccessRedirectUrl { get; set; }
    }
}
