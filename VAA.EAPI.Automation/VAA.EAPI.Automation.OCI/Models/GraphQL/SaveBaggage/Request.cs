namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SaveBaggage
{
    public class Request
    {
        public string checkInId { get; set; }
        public List<SelectedBaggageOffer> selectedBaggageOffer { get; set; }
    }
}