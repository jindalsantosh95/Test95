namespace VAA.EAPI.Automation.Search.Models.SustainabilityMS
{
    public class SustainabiltyResponse
    {
        public string originAirport { get; set; }
        public string destinationAirport { get; set; }
        public string flightNumber { get; set; }
        public string requestId { get; set; }
        public string sourceMethodology { get; set; }
        public string sourceQuality { get; set; }
        public double kilosCo2e { get; set; }
        public double lbsCo2e { get; set; }
        public string source { get; set; }
        public string statusCode { get; set; }
        public string cabin { get; set; }
    }
}