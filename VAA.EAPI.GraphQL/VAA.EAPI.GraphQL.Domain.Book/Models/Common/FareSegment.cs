namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class FareSegments
    {
        public string CabinName { get; set; }
        public string BookingClass { get; set; }
        public bool IsDominantLeg { get; set; }
        public string FlightSegmentRefId { get; set; }
    }
}
