namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SaveReturnDate
{
    public class SaveReturnDateVariables
    {
        public string checkInId { get; set; }
        public string passengerId { get; set; }
        public string returnOn { get; set; }
        public bool noReturnDate { get; set; }
    }
}