using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Enum;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;
using static VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.EnrolCustomerDetails;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment
{
    public class DeDupValidationEnrolment : IDeDupValidationEnrolment
    {
        private readonly IDeDupRepositoryEnrolment _deduprepository;

        public DeDupValidationEnrolment(IDeDupRepositoryEnrolment deduprepository)
        {
            _deduprepository = deduprepository ?? throw new ArgumentNullException(nameof(deduprepository));
        }

        public async Task<List<ErrorDetails>> DeDupValidate(EnrolmentRequestCommand enrolmentRequest)
        {
            List<ErrorDetails> validationErrors = new List<ErrorDetails>();

            if (enrolmentRequest.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty
                && enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                string loyaltyMemberNumber = await _deduprepository.GetVredDetails(enrolmentRequest);
                if (!string.IsNullOrEmpty(loyaltyMemberNumber))
                {
                    var doc = await _deduprepository.GetCustomerDetailWithFcMember(loyaltyMemberNumber);
                    if (doc != null)
                    {
                        if (doc.Count == 1)
                        {
                            if (!Enum.IsDefined(typeof(VredStatusEnum), doc.First().Status.Replace(" ", "")))
                                return ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.VredError2), Guid.NewGuid().ToString(), doc.FirstOrDefault().LoyaltyMemberNumber);

                            return ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.VredError1), Guid.NewGuid().ToString(), doc.FirstOrDefault().LoyaltyMemberNumber);
                        }
                        else if (doc.Count > 1)
                        {
                            return ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.VredError3), Guid.NewGuid().ToString(), string.Join(",", doc.Select(s => s.LoyaltyMemberNumber).ToArray()));
                        }
                    }
                }
            }

            if (enrolmentRequest.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty)
            {
                if (enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebMyaChannelId) || enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.AppMyaChannelId))
                {
                    var countryList = ConfigReader.Get(Constants.Environment.PostCodeMandatoryCountryList).Split("|").ToList();

                    if (countryList.Contains(enrolmentRequest.Data.CustomerDetails?.AddressDetails?.FirstOrDefault()?.Country) && string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails?.AddressDetails?.FirstOrDefault()?.PostalCode))
                    {
                        return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), new Guid().ToString());
                    }
                }

                string matchEmail = await _deduprepository.GetCustomerDetailsWithEmail(enrolmentRequest);
                string matchString = await _deduprepository.GetCustomerDetailsWithMatchString(enrolmentRequest);
                string matchStringAndEmail = await _deduprepository.GetCustomerDetailsWithEmailAndMatchString(enrolmentRequest);

                if (!string.IsNullOrWhiteSpace(matchEmail))
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.EmailidMatchError), Guid.NewGuid().ToString(), matchEmail);


                if (!string.IsNullOrWhiteSpace(matchString))
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.MatchStringError), Guid.NewGuid().ToString(), matchString);

                if (!string.IsNullOrWhiteSpace(matchStringAndEmail))
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.BothEmailidMatchstringError), Guid.NewGuid().ToString(), matchStringAndEmail);
            }
            else
            {
                string MatchEmail = await _deduprepository.GetCustomerDetailsWithEmail(enrolmentRequest);

                if (MatchEmail.Length > 0)
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(ErrorCodes.EmailidMatchError), Guid.NewGuid().ToString(), MatchEmail);
            }

            return validationErrors;
        }
    }
}
