namespace VAA.EAPI.Automation.OCI.Models.Microservices.SaveBaggage
{
    public class CheckedBaggageRequest
    {
        public string BasketId { get; set; }
        public List<CheckedBaggageItem> CheckedBaggage { get; set; }
    }
}