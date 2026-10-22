using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaSsci;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface
{
    public interface IDeltaSsciService
    {
        Task<DeltaSsci.TravelParty> PostTravelPartiesAsync(DeltaSsci.InputObject inputObject);
        Task<DeltaSsci.TravelParty> GetTravelPartiesAsync(string travelPartyId);
        Task<DeltaSsci.TravelParty> PatchTravelPartiesAsync(string travelPartyId, DeltaSsci.SelectPassengerRequest selectPassengerRequest);
        Task<DeltaSsci.PassportData> GetPassportDetailsAsync(string travelPaertyId);
        Task<AddPassportResponse> PostPassportDetailsAsync(string travelPartyId, AddPassportRequest addPassportRequest);
        Task<ContactTracingResponse> SaveContactTracingAsync(string travelPartyId, ContactTracingRequest contactTracingRequest);
        Task<BoardingDocumentsResponse> PostBoardingDocumentsAsync(string travelPartyId);
        Task<TravelParty> PostSeatsAsync(string travelPartyId, AddSeatRequest seatRequest);
        Task<bool> PostBuildTransactionSummaryAsync(string travelPartyId, BuildTransactionSummaryRequest buildTransactionSummaryRequest);
    }
}
