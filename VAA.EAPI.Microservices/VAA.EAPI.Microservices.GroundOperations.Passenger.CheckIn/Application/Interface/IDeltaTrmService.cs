using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface
{
    public interface IDeltaTrmService
    {
        Task<RequirementsResponse> PostRequirementsAsync(RequirementsRequest request);
        Task<RequirementsResponse> GetRequirementsAsync(string requirementsResourceId);
        Task<RequirementFormResponse> PatchSingleRequirementsAsync(string requirementsResourceId, RequirementFormRequest addPassportRequest);
        Task<RequirementFormResponse> PatchBatchRequirementsAsync(string requirementsResourceId, RequirementFormRequest saveEmergencyContactRequest);
    }
}