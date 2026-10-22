namespace VAA.EAPI.Automation.OCI.Models.GraphQL.ContactTracing
{
    public class ContactTracingVariables
    {
        public string checkInId { get; set; }
        public bool applyToAll { get; set; }
        public List<ContactTracingInfo> contactTracings { get; set; }
    }
}
