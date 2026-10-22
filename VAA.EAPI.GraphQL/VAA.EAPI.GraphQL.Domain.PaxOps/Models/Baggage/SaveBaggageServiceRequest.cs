using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class SaveBaggageServiceRequest
    {
        public string TravelPartyId { get; set; }
        public IEnumerable<SelectedBaggageOffer> BaggagePricing { get; set; }

        public static async Task<SaveBaggageServiceRequest> CreateRequestAsync(SaveBaggageRequest source)
        {
            return new SaveBaggageServiceRequest
            {
                TravelPartyId = await CheckInIdHelper.GetTravelPartyId(source.CheckInId),
                BaggagePricing = source.SelectedBaggageOffer
            };
        }
    }
}
