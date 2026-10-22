using System;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class PartyIdentityDocument
    {
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string CountryOfIssue { get; set; }
        public string DocumentIssuer { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string NameOnDocument { get; set; }
        public DateTime? DOBOnDocument { get; set; }
        public string GenderOnDocument { get; set; }


    }
}
