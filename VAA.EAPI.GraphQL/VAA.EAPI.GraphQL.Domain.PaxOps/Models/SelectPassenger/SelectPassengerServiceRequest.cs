using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SavePassport;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.SelectPassenger
{
    public class SelectPassengerServiceRequest
    {
        public string TravelPartyId { get; set; }
        public IEnumerable<string> SelectedPassengerIds { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PnrLocator { get; set; }
        public string OriginCode { get; set; }

        public static async Task<SelectPassengerServiceRequest> CreateRequestAsync(string checkInId, IEnumerable<string> passengerIds)
        {
            return new SelectPassengerServiceRequest
            {
                TravelPartyId = await CheckInIdHelper.GetTravelPartyId(checkInId),
                PnrLocator = await CheckInIdHelper.GetPnrLocator(checkInId),
                OriginCode = await CheckInIdHelper.GetOriginCode(checkInId),
                FirstName = await CheckInIdHelper.GetFirstName(checkInId),
                LastName = await CheckInIdHelper.GetLastName(checkInId),
                SelectedPassengerIds = passengerIds
            };
        }
    }
}