using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository
{
    public interface ICustomerSearchRepository
    {
        public Task AddCustomerSearchDetails(CreateCustomerEntities createCustomerEntities);
        public Task UpdateCustomerSearchDetails(CreateCustomerEntities createCustomerEntities, Customers customers);

    }
}
