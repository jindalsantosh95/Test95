namespace VAA.EAPI.Automation.OCI.Models.GraphQL.ConfirmBaggage
{
    public class ConfirmBaggageRequest
    {
        public object checkInId { get; set; }
        public object paymentReferenceId { get; set; }
        public object paymentStatus { get; set; }
    }
}