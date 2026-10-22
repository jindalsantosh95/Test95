using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IStampingPNRsHandler
    {
        Task StampPNRs(CustomerDTO customerResponse);
    }
}
