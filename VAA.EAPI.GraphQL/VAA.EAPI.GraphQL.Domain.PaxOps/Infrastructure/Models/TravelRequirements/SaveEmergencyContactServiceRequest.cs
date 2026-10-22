using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements
{
    public class SaveEmergencyContactServiceRequest
    {
        public string RequirementsId { get; set; }
        public IEnumerable<EmergencyContact> EmergencyContacts { get; set; }

        public static async Task<SaveEmergencyContactServiceRequest> CreateRequestAsync(string travelRequirementsId, IEnumerable<EmergencyContact> emergencyContacts)
        {
            return new SaveEmergencyContactServiceRequest
            {
                RequirementsId = await TravelRequirementsIdHelper.GetRequirementsId(travelRequirementsId),
                EmergencyContacts = emergencyContacts
            };
        }
    }
}
