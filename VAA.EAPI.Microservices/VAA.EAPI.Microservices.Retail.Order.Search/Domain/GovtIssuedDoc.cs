namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class GovtIssuedDoc
    {
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string CountryOfIssue { get; set; }
        public string CountryOfResidence { get; set; }
        public string ExpirationDate { get; set; }
        public string IssueDate { get; set; }
        public string RegionOfIssuance { get; set; }
    }
}
