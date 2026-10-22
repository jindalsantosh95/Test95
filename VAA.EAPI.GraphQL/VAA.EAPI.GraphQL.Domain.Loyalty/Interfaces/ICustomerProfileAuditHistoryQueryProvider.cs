using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ProfileAudit;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ICustomerProfileAuditHistoryQueryProvider
    {
        Task<CustomerProfileAuditHistoryResponse> GetCustomerProfileAuditHistory(string customerId);
    }
}