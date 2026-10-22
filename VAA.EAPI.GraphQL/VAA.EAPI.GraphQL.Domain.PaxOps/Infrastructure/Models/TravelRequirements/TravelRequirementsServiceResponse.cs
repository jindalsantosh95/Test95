using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements
{
    public class TravelRequirementsServiceResponse
    {
        public string RequirementsId { get; set; }
        public Reference Reference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public bool ApplyEmergencyContactToAllPax { get; set; }
    }
}
