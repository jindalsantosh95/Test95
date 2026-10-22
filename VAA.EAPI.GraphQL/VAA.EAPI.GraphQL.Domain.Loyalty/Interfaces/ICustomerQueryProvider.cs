using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ICustomerQueryProvider
    {
        Task<Customers> GetCustomerDoc(string customerId);

        Task<IEnumerable<CustomerSearch>> GetCustomerSearch(string customerId);
    }
}
