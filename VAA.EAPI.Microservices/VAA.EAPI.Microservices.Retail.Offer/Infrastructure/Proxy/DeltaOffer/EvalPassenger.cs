using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class EvalPassenger
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }
        public IEnumerable<AccountableDocument> AccountableDocuments { get; set; }
    }
}
