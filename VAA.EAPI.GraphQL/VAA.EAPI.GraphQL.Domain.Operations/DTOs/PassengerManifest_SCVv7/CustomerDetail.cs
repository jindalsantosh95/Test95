using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Operations.DTOs.PassengerManifest_SCVv7
{
    public class CustomerDetail
    {
        [GraphQLName("SCVId")]
        public string SCVId { get; set; }
        public string CustomerId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string LapInfant { get; set; }
        public string LapInfantName { get; set; }
        public string LapInfantIndexNumber { get; set; }
        public string ChildAge { get; set; }
        public string CorporateCustomer { get; set; }
        public string CorporateName { get; set; }
        public string StaffCustomer { get; set; }
        public string CrewPositioning { get; set; }
        public string NonRevenue { get; set; }
        public string VholsCustomer { get; set; }
        public string PaxNumber { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string SegmentOriginAirportCode { get; set; }
        public string SegmentDestinationAirportCode { get; set; }
        public string SegmentCurrentActionCode { get; set; }
        public string BookedFirstCabin { get; set; }
        public string BookedFirstClass { get; set; }
        public string BookedFirstSeat { get; set; }
        public string BookedRevenueClassOfService { get; set; }
        public string BookedRevenueCabin { get; set; }
        public string LoyaltyAirline { get; set; }
        public string LoyaltyNumber { get; set; }
        public string LoyaltyTierCode { get; set; }
        [UseSorting]
        [UseFiltering]
        public StaffDetail[] StaffDetails { get; set; }
        public Profile[] Profile { get; set; }
        public Order[] Order { get; set; }
        public Connection[] InboundConnection { get; set; }
        public Connection[] OutboundConnection { get; set; }
        public object Links { get; set; }
    }
}
