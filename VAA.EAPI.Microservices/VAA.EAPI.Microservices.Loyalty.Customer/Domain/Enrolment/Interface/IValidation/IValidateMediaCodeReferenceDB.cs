using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation
{
    public interface IValidateMediaCodeReferenceDB
    {
        public Task<List<ErrorDetails>> ValidateMediaCodeFromReferenceDB(string mediaCode);
    }
}
