using VAA.EAPI.Automation.OCI.Models.GraphQL.ContactTracing;

namespace VAA.EAPI.Automation.OCI.Models.GraphQL.TravelRequirements
{
    public class TravelRequirementsSaveContactTracingVariables
    {
        public string travelRequirementsId { get; set; }
        public string sliceId { get; set; }
        public List<ContactTracingInfo> contactTracing { get; set; }
    }
}
