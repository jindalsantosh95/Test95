using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IValidation
{
    public interface IDeDupValidation
    {
        Task<List<ErrorDetails>> DeDupValidate(EnrolmentRequest enrolmentRequest);
    }
}
