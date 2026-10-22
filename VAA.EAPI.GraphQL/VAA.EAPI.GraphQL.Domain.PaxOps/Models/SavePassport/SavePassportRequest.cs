namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.SavePassport
{
    public class SavePassportRequest
    {
        public string CheckInId { get; set; }
        public string PassengerId { get; set; }
        public Document Document { get; set; }
        public string MRZLine1 { get; set; }
        public string MRZLine2 { get; set; }
        public bool IsScanned { get; set; }

    }
}
