using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Interfaces
{
    public interface IDeDupValidation
    {
        public Task<List<ErrorDetails>> DeDupValidate(CreateCustomerEntities enrolmentRequest);
    }
}
