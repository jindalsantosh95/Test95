using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository
{
    public interface ICustomerRepository
    {
        public Task AddCustomerDetails(CreateCustomerEntities createCustomerEntities);

        public Task UpdateCustomerDetails(CreateCustomerEntities createCustomerEntities, Customers customers);

    }
}
