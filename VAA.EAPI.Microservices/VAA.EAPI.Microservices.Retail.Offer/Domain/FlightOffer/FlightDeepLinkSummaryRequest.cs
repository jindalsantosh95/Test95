using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer
{
    public class DeepLinkSummaryRequest
    {
        public string MetaPartnerId { get; set; }
        public Pos Pos { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public LoyaltyMemberDetails LoyaltyMember { get; set; }
        public IEnumerable<DeepLinkSegment> FlightSegments { get; set; }
        public string CurrentTripIndexId { get; set; }
        public string PromoCode { get; set; }
        public string DominantSegmentBrandId { get; set; }
        public Amount TotalOfferedAmount { get; set; }

    }
}
