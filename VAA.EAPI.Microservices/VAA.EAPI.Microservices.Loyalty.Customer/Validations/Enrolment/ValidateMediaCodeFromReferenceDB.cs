using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment
{
    public class ValidateMediaCodeReferenceDB : IValidateMediaCodeReferenceDB
    {
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public ValidateMediaCodeReferenceDB(ICommonRefDBRepository commonRefDBRepository)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }

        public async Task<List<ErrorDetails>> ValidateMediaCodeFromReferenceDB(string mediaCode)
        {
            List<ErrorDetails> validationErrors = new List<ErrorDetails>();
            List<MediaCodeDetails> mediaCodeDetails = await _commonRefDBRepository.GetMediaCodeDetails();

            if (!mediaCodeDetails.Any(m => string.Equals(m.MediaCode, mediaCode, StringComparison.OrdinalIgnoreCase)))
            {
                validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidMediaCode), Guid.NewGuid().ToString());
            }

            return validationErrors;
        }
    }
}
