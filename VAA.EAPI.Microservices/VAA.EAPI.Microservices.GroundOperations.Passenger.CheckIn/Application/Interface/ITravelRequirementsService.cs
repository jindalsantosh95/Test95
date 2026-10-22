using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface
{
    public interface ITravelRequirementsService
    {
        Task<Result<Response<TravelRequirementsResponse>>> GetTravelRequirementsAsync(TravelRequirementsRequest travelRequirementsRequest);
        Task<Result<Response<TravelRequirementsResponse>>> SavePassportAsync(SavePassportRequest savePassportRequest);
        Task<Result<Response<TravelRequirementsResponse>>> SaveReturnDateAsync(SaveReturnDateRequest saveReturnDateRequest);
        Task<Result<Response<TravelRequirementsResponse>>> SaveEmergencyContactAsync(SaveEmergencyContactRequest saveEmergencyContactRequest);
        Task<Result<Response<TravelRequirementsResponse>>> SaveContactTracingAsync(SaveContactTracingRequest saveContactTracingRequest);
        Task<Result<Response<TravelRequirementsResponse>>> SaveGovernmentFormsAttestationAsync(SaveGovernmentFormsAttestationRequest request);
        Task<Result<Response<TravelRequirementsResponse>>> SaveSecondaryDocumentAttestationAsync(SaveSecondaryDocumentAttestationRequest request);
    }
}
