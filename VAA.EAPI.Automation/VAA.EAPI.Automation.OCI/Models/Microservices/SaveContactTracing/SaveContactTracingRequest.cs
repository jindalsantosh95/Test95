using VAA.EAPI.Automation.OCI.Models.GraphQL.ContactTracing;

namespace VAA.EAPI.Automation.OCI.Models.Microservices.SaveContactTracing
{
    public class SaveContactTracingRequest
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public bool ApplyToAllPassengers { get; set; }
        public List<ContactTracingInfo> ContactTracings { get; set; }
    }
}