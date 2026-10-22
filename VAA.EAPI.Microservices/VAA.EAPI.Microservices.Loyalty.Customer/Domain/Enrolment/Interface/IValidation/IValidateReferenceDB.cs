using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation
{
    public interface IValidateReferenceDB
    {
        public Task<List<ErrorDetails>> ValidateFromReferenceDB(EnrolmentRequestCommand enrolmentRequest);
    }
}
