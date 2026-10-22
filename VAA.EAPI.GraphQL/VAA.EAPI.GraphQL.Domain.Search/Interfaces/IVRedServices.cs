using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface IVRedServices
    {
        Task<VRedResponse> GetVRedPoints(string flyingClubNumber);
    }
}
