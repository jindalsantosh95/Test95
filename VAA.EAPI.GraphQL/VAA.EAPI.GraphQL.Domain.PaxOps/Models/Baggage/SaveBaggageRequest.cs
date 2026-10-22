namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class SaveBaggageRequest
    {
        public string CheckInId { get; set; }
        public string EmailAddress { get; set; }
        public IEnumerable<SelectedBaggageOffer> SelectedBaggageOffer { get; set; }
    }
}
