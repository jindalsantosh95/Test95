using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations
{
    public class DeDupValidation : IDeDupValidation
    {
        private readonly IDeDupRepository _deduprepository;

        public DeDupValidation(IDeDupRepository deduprepository)
        {
            _deduprepository = deduprepository ?? throw new ArgumentNullException(nameof(deduprepository));
        }

        public async Task<List<ErrorDetails>> DeDupValidate(CreateCustomerEntities enrolmentRequest)
        {
            List<ErrorDetails> validationErrors = new List<ErrorDetails>();

            if (enrolmentRequest.CustomerType == CustomerTypeEnum.Loyalty
                && enrolmentRequest.EnrollmentChannel == ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                var doc = await _deduprepository.GetVredDetails(enrolmentRequest);

                if (doc != null && doc.Count == 1)
                {
                    if (!Enum.IsDefined(typeof(VredStatusEnum), doc?.FirstOrDefault()?.Status?.Replace(" ", "")))
                        return ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.VredError2), Guid.NewGuid().ToString(), doc.FirstOrDefault().FcNumber);

                    return ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.VredError1), Guid.NewGuid().ToString(), doc.FirstOrDefault().FcNumber);
                }
                else if (doc != null && doc.Count > 1)
                {
                    return ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.VredError3), Guid.NewGuid().ToString(), string.Join(",", doc.Select(s => s.FcNumber).ToArray()));
                }
            }

            if (enrolmentRequest.CustomerType == CustomerTypeEnum.Loyalty)
            {
                string matchEmail = await _deduprepository.GetCustomerDetailsWithEmail(enrolmentRequest);
                string matchString = await _deduprepository.GetCustomerDetailsWithMatchString(enrolmentRequest);
                string matchStringAndEmail = await _deduprepository.GetCustomerDetailsWithEmailAndMatchString(enrolmentRequest);

                if (matchEmail.Length > 0)
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.EmailidMatchError), Guid.NewGuid().ToString(), matchEmail);

                if (matchString.Length > 0)
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.MatchStringError), Guid.NewGuid().ToString(), matchString);

                if (matchStringAndEmail.Length > 0)
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.BothEmailidMatchstringError), Guid.NewGuid().ToString(), matchStringAndEmail);
            }

            else
            {
                string matchEmail = await _deduprepository.GetCustomerDetailsWithEmail(enrolmentRequest);

                if (matchEmail.Length > 0)
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.EmailidMatchError), Guid.NewGuid().ToString(), matchEmail);
            }

            return validationErrors;
        }
    }
}