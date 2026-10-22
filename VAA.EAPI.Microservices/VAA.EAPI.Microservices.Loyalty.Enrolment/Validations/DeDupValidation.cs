using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Enum;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IValidation;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Validations
{
    public class DeDupValidation : IDeDupValidation
    {
        private readonly IDeDupRepository _deduprepository;
        public DeDupValidation(IDeDupRepository deduprepository)
        {
            _deduprepository = deduprepository ?? throw new ArgumentNullException(nameof(deduprepository));
        }
        public async Task<List<ErrorDetails>> DeDupValidate(EnrolmentRequest enrolmentRequest)
        {
            List<ErrorDetails> validationErrors = new List<ErrorDetails>();

            if (enrolmentRequest.CustomerDetails.CustomerType == Customer.CustomerTypeEnum.Loyalty && enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                var doc = await _deduprepository.GetVredDetails(enrolmentRequest);
                if (doc != null && doc.Count == 1)
                {
                    if (!VredStatusEnum.IsDefined(typeof(VredStatusEnum), doc?.FirstOrDefault()?.Status.Replace(" ", "")))
                    {
                        return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(Constants.ErrorCodes.VredError2), Guid.NewGuid().ToString(), doc.FirstOrDefault().LoyaltyMemberNumber);
                    }
                    else
                    {
                        return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(Constants.ErrorCodes.VredError1), Guid.NewGuid().ToString(), doc.FirstOrDefault().LoyaltyMemberNumber);
                    }
                }
                else if (doc != null && doc.Count > 1)
                {
                    return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(Constants.ErrorCodes.VredError3), Guid.NewGuid().ToString(), string.Join(",", doc.Select(s => s.LoyaltyMemberNumber).ToArray()));
                }
            }
            if (enrolmentRequest.CustomerDetails.CustomerType == Customer.CustomerTypeEnum.Loyalty)
            {
                string MatchEmail = await _deduprepository.GetCustomerDetailsWithEmail(enrolmentRequest);
                string MatchString = await _deduprepository.GetCustomerDetailsWithMatchString(enrolmentRequest);
                string MatchStringAndEmail = await _deduprepository.GetCustomerDetailsWithEmailAndMatchString(enrolmentRequest);
                if (MatchEmail.Length > 0)
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(Constants.ErrorCodes.EmailidMatchError), Guid.NewGuid().ToString(), MatchEmail);
                }
                if (MatchString.Length > 0)
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(Constants.ErrorCodes.MatchStringError), Guid.NewGuid().ToString(), MatchString);
                }
                if (MatchStringAndEmail.Length > 0)
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(Constants.ErrorCodes.BothEmailidMatchstringError), Guid.NewGuid().ToString(), MatchStringAndEmail);
                }
            }

            else
            {
                string MatchEmail = await _deduprepository.GetCustomerDetailsWithEmail(enrolmentRequest);
                if (MatchEmail.Length > 0)
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(Constants.ErrorCodes.EmailidMatchError), Guid.NewGuid().ToString(), MatchEmail);
                }

            }
            return validationErrors;
        }
    }
}
