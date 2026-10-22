using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IVRedServiceProvider
    {
        Task<VRedResponse> GetVirginPoints(string fqtNumber);
    }
}
