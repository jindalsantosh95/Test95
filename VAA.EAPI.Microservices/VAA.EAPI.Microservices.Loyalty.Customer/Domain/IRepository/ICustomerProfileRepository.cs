using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository
{
    public interface ICustomerProfileRepository
    {
        public Task AddCustomerProfileDetails(CreateCustomerEntities createCustomerEntities);
        public Task UpdateCustomerProfileDetails(CreateCustomerEntities createCustomerEntities, CustomerProfile customerProfile);
    }
}