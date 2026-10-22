namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.CheckInMS
{
    public sealed class TravelRequirementsResponse
    {
        public TravelRequirementsReference Reference { get; set; }
        public IEnumerable<TravelRequirementsPassenger> Passengers { get; set; } = [];
        public IEnumerable<TravelRequirementsSlice> Slices { get; set; } = [];
    }

    public sealed class TravelRequirementsReference
    {
        public string PnrLocator { get; set; }
    }

    public sealed class TravelRequirementsPassenger
    {
        public string PassengerFirstNin { get; set; }
        public string PassengerLastNin { get; set; }
        public IEnumerable<SliceRequirement> SliceRequirements { get; set; } = [];
    }

    public sealed class SliceRequirement
    {
        public string SliceId { get; set; }
        public string SecondaryDocumentStatus { get; set; }
    }

    public sealed class TravelRequirementsSlice
    {
        public string SliceId { get; set; }
        public TravelRequirementsAirport Origin { get; set; }
        public TravelRequirementsAirport Destination { get; set; }
    }

    public sealed class TravelRequirementsAirport
    {
        public string Code { get; set; }
    }
}
