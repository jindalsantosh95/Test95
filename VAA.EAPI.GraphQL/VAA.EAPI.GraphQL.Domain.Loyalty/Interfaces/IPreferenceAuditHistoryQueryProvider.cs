using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IPreferenceAuditHistoryQueryProvider
    {
        Task<PreferenceAuditHistoryResponse> GetPreferenceAuditHistory(string customerId);
    }
}