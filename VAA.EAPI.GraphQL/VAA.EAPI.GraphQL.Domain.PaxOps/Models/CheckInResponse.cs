using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    public class CheckInResponse
    {
        public string CheckInId { get; set; }
        public Reference Reference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public Slice CurrentSlice { get; set; }
        public OnlineCheckInStatus OnlineCheckInStatus { get; set; }
        public bool ApplyEmergencyContactToAllPax { get; set; }
        public bool ApplyContactTracingToAllPax { get; set; }
        public bool ShowBaggageAllowance { get; set; }

        public static async Task<CheckInResponse> CheckInResponseAsync(Infrastructure.Models.CheckInMS.CheckInResponse response, string existingCheckInId)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new CheckInResponse
            {
                CheckInId = response.OnlineCheckInStatus.Available == true ? await CheckInIdHelper.CreateCheckInId(existingCheckInId, response?.RequirementsId, true, response?.TravelPartyId, true, response?.Reference?.PnrLocator, response?.CurrentSlice?.Origin?.Code, response?.Passengers?.Select(psg => psg.PassengerId), existingCheckInId.IsNullOrEmpty() ? response?.Passengers?.FirstOrDefault()?.GivenNames : null, existingCheckInId.IsNullOrEmpty() ? response?.Passengers?.FirstOrDefault()?.Surname : null) : null,
                Reference = response.Reference != null ? (Reference)response.Reference : null,
                Passengers = response.Passengers?.Select(p => (Passenger)p),
                CurrentSlice = response.CurrentSlice != null ? (Slice)response.CurrentSlice : null,
                OnlineCheckInStatus = response.OnlineCheckInStatus != null ? (OnlineCheckInStatus)response.OnlineCheckInStatus : null,
                ApplyEmergencyContactToAllPax = response.ApplyEmergencyContactToAllPax,
                ApplyContactTracingToAllPax = response.ApplyContactTracingToAllPax,
                ShowBaggageAllowance = response.ShowBaggageAllowance
            };
        }
    }
}