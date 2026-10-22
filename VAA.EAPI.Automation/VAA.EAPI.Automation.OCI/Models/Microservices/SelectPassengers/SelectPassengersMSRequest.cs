namespace VAA.EAPI.Automation.OCI.Models.Microservices.SelectPassengers
{
    public class SelectPassengersMSRequest
    {
        public string TravelPartyId { get; set; }
        public List<string> SelectedPassengerIds { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PnrLocator { get; set; }
        public string OriginCode { get; set; }
    }
}