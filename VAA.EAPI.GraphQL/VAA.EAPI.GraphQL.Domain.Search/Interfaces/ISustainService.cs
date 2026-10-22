using VAA.EAPI.GraphQL.Domain.Search.Models.Sustain;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface ISustainService
    {
        Task<IEnumerable<SustainMSResponse>> RetrieveSustainResponseAsync(SustainMSRequest sustainMSRequest);
        Task<IEnumerable<IEnumerable<SustainMSResponse>>> RetrieveSustainResponseCollectionAsync(IEnumerable<SustainMSRequest> sustainMSRequests);
    }
}