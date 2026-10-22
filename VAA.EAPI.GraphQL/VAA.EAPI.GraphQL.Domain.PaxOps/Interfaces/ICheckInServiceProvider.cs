using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SavePassport;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SaveReturnDate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SelectPassenger;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces
{
    public interface ICheckInServiceProvider
    {
        Task<CheckInResponse> InitiateCheckInAsync(VAA.EAPI.GraphQL.Domain.PaxOps.Models.CheckInRequest checkInRequest, bool mapHttpFailureToResponse = true);
        Task<CheckInResponse> SelectPassengersAsync(SelectPassengerServiceRequest selectPassengerRequest, string? checkInId = null, bool mapHttpFailureToResponse = true);
        Task<CheckInResponse> SavePassportAsync(SavePassportServiceRequest savePassportRequest, string? checkInId = null);
        Task<CheckInResponse> SaveContactTracingAsync(SaveContactTracingServiceRequest contactTracingRequest, string? checkInId = null);
        Task<CheckInResponse> SaveEmergencyContactDetails(EmergencyContactServiceRequest contactTracingRequest, string? checkInId = null);
        Task<CheckInResponse> SaveReturnDateAsync(SaveReturnDateServiceRequest saveReturnDateRequest, string? checkInId = null);
        Task<Infrastructure.Models.CheckInMS.BoardingPassResponse> GenerateBoardingPassAsync(BoardingPassServiceRequest request);
        Task<Infrastructure.Models.CheckInMS.BoardingPassResponse> GetBoardingPassByCheckInIdAsync(string travelPartyId);
        Task<Infrastructure.Models.CheckInMS.BoardingPassResponse> RetrieveBoardingPassAsync(BoardingPassRequest boardingPassRequest);
        Task<CheckInResponse> GetCheckInResponseAsync(string travelPartyId);
        Task LogTransactionSummaryAsync(Models.TransactionSummaryRequest request);
    }
}
