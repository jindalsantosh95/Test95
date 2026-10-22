namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SavePassport
{
    public class SavePassportVariables
    {
        public string checkInId { get; set; }
        public string passengerId { get; set; }
        public Document document { get; set; }
        public string mRZLine1 { get; set; }
        public string mRZLine2 { get; set; }
        public bool isScanned { get; set; }
    }
}