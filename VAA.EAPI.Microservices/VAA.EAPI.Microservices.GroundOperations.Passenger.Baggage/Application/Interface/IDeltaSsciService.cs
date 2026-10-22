using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Interface
{
    public interface IDeltaSsciService
    {
        Task<BagsData> GetBaggageInfoAsync(string travelPartyId);
        Task<AddBagsResponse> SaveBaggageAsync(string travelPartyId, UpdateBagsRequest request);
        Task<AddBagsViewData> CheckPaymentAsync(string travelPartyId, UpdateBagsRequest request);
        Task<BagsPurchaseReceipt> ConfirmBaggageAsync(string travelPartyId, PurchaseRequest request);
    }
}
