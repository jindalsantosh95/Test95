using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IAddressHistoryQueryProvider
    {
        Task<IEnumerable<AddressHistory>> GetAddressHistory(string customerId);
    }
}
