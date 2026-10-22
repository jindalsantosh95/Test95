using VAA.EAPI.Automation.OCI.Models.GraphQL.SavePassport;

namespace VAA.EAPI.Automation.OCI.Models.Microservices.SavePassport
{
    public class SavePassportMSRequest
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public string PassengerId { get; set; }
        public Document Document { get; set; }
        public string MRZLine1 { get; set; }
        public string MRZLine2 { get; set; }
        public bool IsScanned { get; set; }
    }
}