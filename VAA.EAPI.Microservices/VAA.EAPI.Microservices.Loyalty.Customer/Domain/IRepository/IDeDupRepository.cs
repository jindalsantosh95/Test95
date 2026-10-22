using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository
{
    public interface IDeDupRepository
    {
        public Task<string> GetCustomerDetailsWithMatchString(CreateCustomerEntities enrolmentRequest);
        public Task<string> GetCustomerDetailsWithEmail(CreateCustomerEntities enrolmentRequest);
        public Task<string> GetCustomerDetailsWithEmailAndMatchString(CreateCustomerEntities enrolmentRequest);
        public Task<List<CreateCustomerEntities>> GetVredDetails(CreateCustomerEntities enrolmentRequest);
    }
}
