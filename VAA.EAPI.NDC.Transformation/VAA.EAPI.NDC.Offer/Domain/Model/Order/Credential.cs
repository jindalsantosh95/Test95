using System;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Credential
    {
        public string Type { get; set; }
        public string Issuer { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public Link CredentialIDP { get; set; }


    }
}
