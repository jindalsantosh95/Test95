using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing
{
    public class SaveContactTracingServiceRequest
    {
        public bool ApplyToAllPassengers { get; set; }
        public IEnumerable<ContactTracing> ContactTracings { get; set; }
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public IEnumerable<string> SelectedPassengersIds { get; set; }

        public static async Task<SaveContactTracingServiceRequest> CreateRequestAsync(string checkInId, bool applyToAll, IEnumerable<ContactTracing> contactTracings)
        {
            return new SaveContactTracingServiceRequest
            {
                TravelPartyId = await CheckInIdHelper.GetTravelPartyId(checkInId),
                RequirementsId = await CheckInIdHelper.GetRequirementsId(checkInId),
                SelectedPassengersIds = await CheckInIdHelper.GetSelectedPassengers(checkInId),
                ApplyToAllPassengers = applyToAll,
                ContactTracings = contactTracings
            };
        }
    }
}