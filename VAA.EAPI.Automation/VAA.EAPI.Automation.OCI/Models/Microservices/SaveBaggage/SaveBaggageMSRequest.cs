namespace VAA.EAPI.Automation.OCI.Models.Microservices.SaveBaggage
{
    public class SaveBaggageMSRequest
    {
        public string travelPartyId { get; set; }
        public List<BaggagePricing> BaggagePricing { get; set; }
    }
}