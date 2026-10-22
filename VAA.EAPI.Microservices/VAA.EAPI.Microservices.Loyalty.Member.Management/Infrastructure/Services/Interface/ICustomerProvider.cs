using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface
{
    public interface ICustomerProvider
    {
        Task<CustomerDTO> GetCustomer(string customerId, string memberNumber);
        Task<Customer> GetCustomerDocument(string customerId);
        Task<CustomerSearch> GetCustomerSearchDocument(string memberNumber);
    }
}
