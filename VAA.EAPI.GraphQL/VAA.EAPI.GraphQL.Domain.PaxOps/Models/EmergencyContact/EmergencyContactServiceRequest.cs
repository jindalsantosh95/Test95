using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact
{
    public class EmergencyContactServiceRequest
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public IEnumerable<string> SelectedPassengersIds { get; set; }
        public bool ApplyToAllPassengers { get; set; }
        public IEnumerable<EmergencyContact> EmergencyContacts { get; set; }

        public static async Task<EmergencyContactServiceRequest> CreateRequestAsync(string checkInId, bool applyToAllPassengers, IEnumerable<EmergencyContact> emergencyContacts)
        {
            return new EmergencyContactServiceRequest
            {
                TravelPartyId = await CheckInIdHelper.GetTravelPartyId(checkInId),
                RequirementsId = await CheckInIdHelper.GetRequirementsId(checkInId),
                SelectedPassengersIds = await CheckInIdHelper.GetSelectedPassengers(checkInId),
                ApplyToAllPassengers = applyToAllPassengers,
                EmergencyContacts = emergencyContacts
            };
        }
    }
}
