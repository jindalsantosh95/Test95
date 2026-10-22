using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IAmexCenturionAuditHistoryQueryProvider
    {
        Task<AmexCenturionAuditHistoryResponse> GetAmexCenturionAuditHistory(string customerId);
    }
}
