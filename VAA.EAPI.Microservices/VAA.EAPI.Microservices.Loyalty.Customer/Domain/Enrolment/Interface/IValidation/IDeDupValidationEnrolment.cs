using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation
{
    public interface IDeDupValidationEnrolment
    {
        Task<List<ErrorDetails>> DeDupValidate(EnrolmentRequestCommand enrolmentRequest);
    }
}
