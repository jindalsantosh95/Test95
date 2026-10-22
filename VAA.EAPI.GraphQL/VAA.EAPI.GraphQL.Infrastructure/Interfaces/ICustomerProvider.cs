using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface ICustomerProvider
    {
        Task<CustomerDTO> GetCustomer(string customerId, string memberNumber);
    }
}
