namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class TravelDocument
    {
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string DateOfBirth { get; set; }
        public string CountryOfIssue { get; set; }
        public string CountryOfResidence { get; set; }
        public PersonName PersonName { get; set; }
        public string ExpirationDate { get; set; }
        public string Gender { get; set; }
        public string IssueDate { get; set; }
        public bool Verified { get; set; }
        public string RegionOfIssuance { get; set; }
        public string Nationality { get; set; }
    }
}
