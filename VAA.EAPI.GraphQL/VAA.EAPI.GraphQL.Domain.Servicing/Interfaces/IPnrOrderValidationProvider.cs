using VAA.EAPI.GraphQL.Domain.Servicing.Models.PnrOrderValidation;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Interfaces
{
    public interface IPnrOrderValidationProvider
    {
        Task<PnrOrderValidationResponse> ValidateAsync(PnrOrderValidationRequest request);
    }
}
