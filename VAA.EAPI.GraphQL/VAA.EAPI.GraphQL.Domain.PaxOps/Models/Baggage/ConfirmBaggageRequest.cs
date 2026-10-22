namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class ConfirmBaggageRequest
    {
        public string CheckInId { get; set; }
        public string PaymentReferenceId { get; set; }
        public string PaymentStatus { get; set; }
    }
}
