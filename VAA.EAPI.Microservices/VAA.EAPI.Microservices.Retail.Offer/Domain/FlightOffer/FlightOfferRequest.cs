using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer
{
    public class FlightOfferRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public FlightsearchRequest FlightsearchRequest { get; set; }
        public List<Customer> CustomerDetails { get; set; }
        // 2024-03-26: Temporary feature flag as we transition to GraphQL. Remove in task 542882.
        public bool? FeatureFlagUseGraphQl { get; set; }
        public LoyaltyMemberDetails LoyaltyMember { get; set; }
        public string CurrencyCode { get; set; }
    }
}