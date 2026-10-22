using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IValidation
{
    public interface IValidateReferenceDB
    {
        public Task<List<ErrorDetails>> ValidateFromReferenceDB(EnrolmentRequest enrolmentRequest);
    }
}
