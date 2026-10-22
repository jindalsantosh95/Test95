
namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain
{
    public class DocumentDetail
    {
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string DocumentId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentTitle { get; set; }
        public string DocumentFirstName { get; set; }
        public string DocumentMiddleName { get; set; }
        public string DocumentLastName { get; set; }
        public string IssuingCountry { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string Nationality { get; set; }
        public string CountryOfResidence { get; set; }
    }
}
