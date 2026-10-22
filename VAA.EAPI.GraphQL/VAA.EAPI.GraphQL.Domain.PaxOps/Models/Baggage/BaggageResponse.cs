using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class BaggageResponse
    {
        public string CheckInId { get; set; }
        public IEnumerable<PassengerAllowance> Passengers { get; set; }
        public bool ShowBaggageAllowance { get; set; }
        public AllAboutBags AllAboutBags { get; set; }
        public string BaggageLearnMoreUrl { get; set; }
        public string? EmailAddress { get; set; }

        public static async Task<BaggageResponse> BaggageResponseAsync(Infrastructure.Models.BaggageMS.BaggageResponse response, string existingCheckInId)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new BaggageResponse
            {
                CheckInId = await CheckInIdHelper.CreateCheckInId(existingCheckInId, null, false, response.TravelPartyId, true),
                Passengers = response?.Passengers,
                ShowBaggageAllowance = response.ShowBaggageAllowance,
                AllAboutBags = response?.AllAboutBags,
                BaggageLearnMoreUrl = response.BaggageLearnMoreUrl
            };
        }
    }
}