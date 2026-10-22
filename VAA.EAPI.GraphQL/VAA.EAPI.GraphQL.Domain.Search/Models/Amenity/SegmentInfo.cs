using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.Amenity
{
    public class SegmentInfo
    {
        public int? SegmentNum { get; set; }
        [GraphQLDeprecated("This class represents a segment, not a leg. Use the SegmentNum property instead.")]
        public int? Leg => SegmentNum;
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public LocalDateTime? FlightDepartureTime { get; set; }
        public int? FlightNumber { get; set; }
        public string? FareBasisCode { get; set; }
        public string? FareFamilyType { get; set; }
        public string? OperatedBy { get; set; }
        public string? OperatedByCode { get; set; }
        public string? DominantCabinName { get; set; }
        public string? HeroImageURI { get; set; }
        public string? SeatSelection { get; set; }
        public string? SeatPitch { get; set; }
        public string? CheckedBags { get; set; }
        public string? CarryOnBags { get; set; }
        public string? BookingClass { get; set; }
        public string? FreshFoods { get; set; }
        public string? Beverages { get; set; }
        public string? BoardingPriority { get; set; }
        public string? CheckInPriority { get; set; }
        public string? LoungeAccess { get; set; }
        public bool? DominantSegment { get; set; }
        [GraphQLDeprecated("This class represents a segment, not a leg. Use the DominantSegment property instead.")]
        public bool? DominantLeg => DominantSegment;
    }
}
