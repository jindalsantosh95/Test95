namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class FlightSearchRequest
    {
        public IEnumerable<SearchOriginDestination>? SearchOriginDestinations { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public bool? BundleOffer { get; set; }
        [GraphQLDescription("Return results for offers for a specific date or set of dates.")]
        public bool? OfferSearch { get; set; }
        [GraphQLDescription("Return lead in prices for the same route on different days. For return flights, the trip duration will be consistent.")]
        public bool? CalendarSearch { get; set; }
        [GraphQLDescription("Return results for offers for a specific date or set of dates.")]
        public bool? FlexiDateSearch { get; set; }
        public string? PromoCode { get; set; }
        public bool? AwardSearch { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public bool? NonStopOnly { get; set; }
        public string CurrentTripIndexId { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public IEnumerable<string>? ClassPreferences { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public int? NumberStops { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public string? PointOfSaleCityCode { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public bool? CheckInBaggageAllowance { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public bool? CarryOnBaggageAllowance { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public bool? RefundableOnly { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public IEnumerable<string>? PermittedCarrierCode { get; set; }
        [GraphQLDeprecated("This has no effect. Consider passing PromoCode.")]
        public IEnumerable<DiscountCertificate>? DiscountCertificate { get; set; }
        public string? BrandId { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public IEnumerable<CabinType>? CabinTypes { get; set; }
        [GraphQLDeprecated("This has no effect")]
        public IEnumerable<string>? FareFamilies { get; set; }
        public string? OfferID { get; set; }
    }
}