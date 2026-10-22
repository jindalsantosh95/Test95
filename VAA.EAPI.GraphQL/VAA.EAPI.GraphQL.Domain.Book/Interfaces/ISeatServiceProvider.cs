using VAA.EAPI.GraphQL.Domain.Book.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface ISeatServiceProvider
    {
        Task<SeatMSResponse> RetrieveSeatMapAsync(SeatMSRequest seatMapRequest);
        Task<SeatMSResponse> RetrieveSeatOfferDetailsAsync(string offerId);
        Task<SeatMSResponse> RetrieveSeatMapWithSelectionAsync(SeatMapSelectionMSRequest seatMapRequest);
    }
}