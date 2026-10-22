using VAA.EAPI.GraphQL.Domain.Trips.Models.Content;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IContentServiceProvider
    {
        Task<IEnumerable<string>> GetContentDetails(string? contentFragmentId, ContentRequest? contentRequest = null);
    }
}
