using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface ISeatProvider
    {
        Task<SeatResponse> RetrieveSeatMapAsync(SeatRequest seatMapRequest);
        Task<SeatResponse> RetrieveSeatOfferDetailsAsync(string seatOfferId);
        Task<SeatResponse> RetrieveSeatMapWithSelectionAsync(SeatMapSelectionRequest seatMapRequest);
        Task<SeatDeletionResponse> DeleteSeatAsync(SeatDeletionRequest request);
        Task<SeatFulfilmentResponse> SeatFulfilmentAsync(SeatFulfilmentRequest request);
    }
}