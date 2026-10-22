namespace VAA.EAPI.Automation.OCI.Models.Microservices.CheckIn
{
    public class CheckInMSRequest
    {
        public string PNRLocator { get; set; }
        public string ETicketNumber { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string OriginCode { get; set; }
    }
}