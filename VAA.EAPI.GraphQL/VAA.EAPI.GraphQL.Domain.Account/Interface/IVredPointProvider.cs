using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IVredPointProvider
    {
        Task<VRedResponse> VredPointBalance(string memberNumber);
    }
}
