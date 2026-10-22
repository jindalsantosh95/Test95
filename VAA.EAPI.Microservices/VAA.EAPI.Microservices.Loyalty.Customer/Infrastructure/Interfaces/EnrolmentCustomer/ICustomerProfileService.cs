using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer
{
    public interface ICustomerProfileService
    {
        public Task<IEnumerable<Customers>> GetCustomerDetails(CustomerProfileRequest customerProfileRequest);
    }
}
