using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface
{
    public interface ICustomerProvider
    {
        Task<CustomerDTO> GetCustomer(string customerId, string memberNumber);
    }
}
