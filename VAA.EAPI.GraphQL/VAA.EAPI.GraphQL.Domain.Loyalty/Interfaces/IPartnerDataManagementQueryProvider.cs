using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IPartnerDataManagementQueryProvider
    {
        Task<VSFlownDetailsResponse> GetVSRetroFlownData(GetVSRetroFlownDataRequest request);
        Task<EditVSRetroFlownDataResponse> UpdateVSRetroFlownDetails(EditVSRetroFlownDataRequest request);
        Task<ValidateVSClaimInputsResponse> ValidateVSClaimInputs(ValidateVSClaimInputsRequest request);
        Task<string> UpdateVSRetroFlownStatus(TicketStatusRequest request);
    }
}

