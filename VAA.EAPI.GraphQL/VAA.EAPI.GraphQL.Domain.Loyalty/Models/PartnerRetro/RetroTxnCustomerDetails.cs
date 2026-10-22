namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class RetroTxnCustomerDetails
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string DocumentFirstName { get; set; }
        public string DocumentSurname { get; set; }
        public string PrimaryDocumentType { get; set; }
        public string PrimaryDocumentStatus { get; set; }
        public string SecondaryDocumentType { get; set; }
        public string SecondaryDocumentStatus { get; set; }
        public string TierCode { get; set; }
        public string MemberType { get; set; }
        public string MemberStatus { get; set; }
        public string EmailAddress { get; set; }
        public string DateOfBirth { get; set; }
    }
}
