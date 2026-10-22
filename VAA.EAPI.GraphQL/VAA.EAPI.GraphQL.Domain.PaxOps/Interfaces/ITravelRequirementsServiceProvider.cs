using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces
{
    public interface ITravelRequirementsServiceProvider
    {
        Task<TravelRequirementsServiceResponse> GetTravelRequirementsAsync(TravelRequirementsRequest request);
        Task<TravelRequirementsServiceResponse> SavePassportAsync(SavePassportServiceRequest request);
        Task<TravelRequirementsServiceResponse> SaveReturnDateAsync(TravelRequirementsSaveReturnDateServiceRequest request);
        Task<TravelRequirementsServiceResponse> SaveEmergencyContactAsync(SaveEmergencyContactServiceRequest request);
        Task<TravelRequirementsServiceResponse> SaveContactTracingAsync(TravelRequirementsSaveContactTracingServiceRequest request);
        Task<TravelRequirementsServiceResponse> SaveGovernmentFormsAttestationAsync(TravelRequirementsSaveGovernmentFormsAttestationServiceRequest request);
        Task<TravelRequirementsServiceResponse> SaveSecondaryDocumentAttestationAsync(TravelRequirementsSaveSecondaryDocumentAttestationServiceRequest request);
    }
}
