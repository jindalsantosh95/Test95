using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public class TravelRequirementsResponse
    {
        public string RequirementsId { get; set; }
        public Reference Reference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public bool ApplyEmergencyContactToAllPax { get; set; }

        public static explicit operator TravelRequirementsResponse(DeltaTrm.RequirementsResponse source) => MapResponse(source.RequirementsResourceId, source.TravelRequirements);

        public static explicit operator TravelRequirementsResponse(DeltaTrm.RequirementFormResponse source) => MapResponse(source.RequirementsResourceId, source.TravelRequirements);

        private static TravelRequirementsResponse MapResponse(string requirementsId, DeltaTrm.TravelRequirements travelRequirements)
        {
            var passengers = travelRequirements.Passengers
                .Select(pax => (Passenger)(pax, travelRequirements.Trips))
                .ToList();

            return new TravelRequirementsResponse
            {
                RequirementsId = requirementsId,
                Reference = (Reference)travelRequirements,
                Passengers = passengers,
                Slices = travelRequirements.Trips.Select(trip => (Slice)trip),
                ApplyEmergencyContactToAllPax = EmergencyContactHelper.ShouldApplyEmergencyContactToAllPassengers(passengers)
            };
        }
    }
}
