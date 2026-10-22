using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class CustomerDetails
    {
        public string Title { get; set; }
        public string CustId { get; set; }
        public string PTC { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public Contacts Contacts { get; set; }
        public Addresses Addresses { get; set; }
        public IEnumerable<TravelDocument> TravelDocuments { get; set; }
        public LoyaltyMemberDetails LoyaltyMemberDetails { get; set; }
        public string AssociatedWithAdult { get; set; }
        public IEnumerable<GovtIssuedDoc> GovtIssuedDocs { get; set; }
    }
}
