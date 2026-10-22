using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.DuplicateCheck
{
    public interface IDeDupRepository
    {
        public Task<string> GetCustomerDetailsWithMatchString(EnrolmentRequest enrolmentRequest);
        public Task<string> GetCustomerDetailsWithEmail(EnrolmentRequest enrolmentRequest);
        public Task<string> GetCustomerDetailsWithEmailAndMatchString(EnrolmentRequest enrolmentRequest);
        public Task<List<Infrastructure.Repositories.DuplicateCheck.Entities.Customer>> GetVredDetails(EnrolmentRequest enrolmentRequest);
    }
}
