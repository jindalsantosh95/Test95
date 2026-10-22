using VAA.EAPI.Automation.OCI.Models.GraphQL.SavePassport;

namespace VAA.EAPI.Automation.OCI.Models.GraphQL.TravelRequirements
{
    public class SavePassportTravelRequirementsRequestInput
    {
        public string travelRequirementsId { get; set; }
        public string passengerId { get; set; }
        public bool isScanned { get; set; }
        public Document document { get; set; }
    }
}
