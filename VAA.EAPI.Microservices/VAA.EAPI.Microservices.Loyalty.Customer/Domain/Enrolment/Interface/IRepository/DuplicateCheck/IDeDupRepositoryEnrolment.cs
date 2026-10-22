using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck
{
    public interface IDeDupRepositoryEnrolment
    {
        public Task<string> GetCustomerDetailsWithMatchString(EnrolmentRequestCommand enrolmentRequest);
        public Task<string> GetCustomerDetailsWithEmail(EnrolmentRequestCommand enrolmentRequest);
        public Task<string> GetCustomerDetailsWithEmailAndMatchString(EnrolmentRequestCommand enrolmentRequest);

        public Task<List<Infrastructure.Repositories.Enrolment.DuplicateCheck.Entities.Customer>> GetCustomersWithEmail(EnrolmentRequestCommand enrolmentRequest);
        public Task<List<Infrastructure.Repositories.Enrolment.DuplicateCheck.Entities.Customer>> GetCustomerDetailWithFcMember(string LoyaltyMemberNumber);
        public Task<string> GetVredDetails(EnrolmentRequestCommand enrolmentRequest);
        public Task<string> GetCustomersWithEmail(string emailId);
        public Task<List<Infrastructure.Repositories.Enrolment.DuplicateCheck.Entities.Customer>> GetCustomersDetailsWithEmail(string emailId);

    }
}
