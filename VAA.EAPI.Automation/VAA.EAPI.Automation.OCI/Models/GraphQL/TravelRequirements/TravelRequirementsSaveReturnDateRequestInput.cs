namespace VAA.EAPI.Automation.OCI.Models.GraphQL.TravelRequirements
{
    public class TravelRequirementsSaveReturnDateRequestInput
    {
        public string passengerId { get; set; }
        public string travelRequirementsId { get; set; }
        public string returnOn { get; set; }
        public bool noReturnDate { get; set; }
    }
}
