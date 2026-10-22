using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Channel;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;
using CustomerTypeEnum = VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.EnrolCustomerDetails.CustomerTypeEnum;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment
{
    public static class ValidatePayload
    {
        public static IEnumerable<ErrorDetails> Validate(EnrolmentRequestCommand enrolmentRequest, string channelId, List<string> blackListedEmails)
        {
            string guid = Guid.NewGuid().ToString();
            List<Channel> channeldetails = ChannelInformation.GetchannelDetails();
            IEnumerable<ErrorDetails> validationErrors = new List<ErrorDetails>();

            bool IsValidChannel = ValidateChannel.Validate(channelId);
            if (!IsValidChannel)
            {
                validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidChannelError), guid);
                return validationErrors;
            }

            if (ConfigReader.Get(Constants.Environment.PowerAppChannelID) == channelId && string.IsNullOrEmpty(enrolmentRequest?.Data?.ChannelUserId))
            {
                validationErrors = ErrorMapperHelper.MapValidationError(ErrorCodes.MissingChanneluserIDError, guid);
                return validationErrors;
            }
            else if (ConfigReader.Get(Constants.Environment.PowerAppChannelID) == channelId && enrolmentRequest?.Data?.CustomerDetails?.AddressDetails != null && enrolmentRequest.Data.CustomerDetails.AddressDetails.Any(a => string.IsNullOrEmpty(a.AddressVerified.ToString())) && enrolmentRequest?.Data?.CustomerDetails?.CustomerType == CustomerTypeEnum.Loyalty)
            {
                validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), guid);
                return validationErrors;
            }

            if (enrolmentRequest.Data.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty && enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                if (enrolmentRequest.Data.CustomerDetails.LoyaltyMember == null || enrolmentRequest.Data.CustomerDetails.LoyaltyMember.MemberType == null || Convert.ToString(enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember.MemberType.Value) != MemberTypes.Individual.ToString())
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidMemberType), guid);
                    return validationErrors;
                }

            }

            bool isEligibleMemberType = ValidateChannel.Validate(channelId);
            if (!isEligibleMemberType)
            {
                validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidChannelError), guid);
                return validationErrors;
            }

            if (enrolmentRequest?.Data?.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty)
            {
                bool isValidCustomerDob = CheckDateFormat.IsValidDate(enrolmentRequest.Data.CustomerDetails.DateOfBirth);
                if (!isValidCustomerDob)
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidDateFromat), guid);
                    return validationErrors;
                }
            }

            if (enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails != null && enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.DateOfBirth != null && enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.VredChannelId))
            {

                bool isValidCustomerGuardianDob = CheckDateFormat.IsValidDate(enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails.DateOfBirth);
                if (!isValidCustomerGuardianDob)
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidDateFromat), guid);
                    return validationErrors;
                }

            }
            if (channeldetails.Where(c => c.Code == enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateCustomerAge)
            {
                int customerAgelimit = ConfigReader.Get<int>(Constants.Environment.CustomerAgeLimit);
                string customerDOB = enrolmentRequest?.Data?.CustomerDetails?.DateOfBirth;
                bool IsValidAge = ValidateAge.Validate(customerDOB, customerAgelimit);
                if (!IsValidAge)
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidCustomerAge), guid);
                    return validationErrors;
                }
            }
            if (enrolmentRequest.Data.CustomerDetails.LoyaltyMember?.GuardianDetails != null && enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.DateOfBirth != null && enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                int parentAgelimit = ConfigReader.Get<int>(Constants.Environment.ParentAgeLimit);
                string parentDOB = enrolmentRequest.Data.CustomerDetails.LoyaltyMember.GuardianDetails.DateOfBirth;
                bool IsParentValidAge = ValidateAge.Validate(parentDOB, parentAgelimit);
                if (!IsParentValidAge)
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidParentAge), guid);
                    return validationErrors;
                }
            }
            if ((enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.DeltaChannelID)) || enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.WebMyaChannelId)) || enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.AppMyaChannelId))) && enrolmentRequest.Data.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty)
            {

                if (enrolmentRequest.Data.CustomerDetails.AddressDetails.Any(a => string.IsNullOrEmpty(a.AddressVerified.ToString())))
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), guid);
                    return validationErrors;
                }
            }

            if (enrolmentRequest.Data.CustomerDetails.ContactDetails != null && enrolmentRequest.Data.CustomerDetails.ContactDetails.EmailDetails != null && channeldetails.Where(c => c.Code == enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateEmail)
            {
                foreach (Email email in enrolmentRequest.Data.CustomerDetails.ContactDetails.EmailDetails)
                {
                    bool IsValidEmailId = ValidateEmail.Validate(email.EmailId);
                    if (!IsValidEmailId)
                    {
                        validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidEmailFromat), guid);
                        return validationErrors;
                    }
                }

                if (enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember != null && enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember.GuardianDetails != null &&
                  enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember.GuardianDetails.EmailDetails != null &&
                  enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember.GuardianDetails.EmailDetails.EmailId != null &&
                  channeldetails.Where(c => c.Code == enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateEmail)
                {
                    bool IsValidEmailId = ValidateEmail.Validate(enrolmentRequest.Data.CustomerDetails.LoyaltyMember.GuardianDetails.EmailDetails.EmailId);
                    if (!IsValidEmailId)
                    {
                        validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidEmailFromat), guid);
                        return validationErrors;
                    }
                }
            }

            if (enrolmentRequest.Data.CustomerDetails.ContactDetails != null && enrolmentRequest?.Data?.CustomerDetails?.ContactDetails.EmailDetails != null)
            {
                foreach (Email email in enrolmentRequest.Data.CustomerDetails.ContactDetails.EmailDetails)
                {
                    if (blackListedEmails.Any(x => x.Is(email.EmailId)))
                    {
                        validationErrors = ErrorMapperHelper.MapValidationError(ErrorCodes.BlacklistedEmailidError, guid);

                        return validationErrors;
                    }
                }
            }

            if (enrolmentRequest.Data.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty)
            {
                int age = DateTime.Now.Year - Convert.ToDateTime(enrolmentRequest?.Data?.CustomerDetails?.DateOfBirth).Year;
                bool GuardianTerms = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.GuardianTermsAndConditionsAccepted ?? false;

                if (age < ConfigReader.Get<int>(Constants.Environment.EnrolmentAgeLimit) && GuardianTerms && !enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.DeltaChannelID)) && !enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.WebMyaChannelId)) && !enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.AppMyaChannelId)) && !enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.PowerAppChannelID)) ||
                   age < ConfigReader.Get<int>(Constants.Environment.EnrolmentAgeLimit) && !GuardianTerms && !enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.DeltaChannelID)) && !enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.WebMyaChannelId)) && !enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.AppMyaChannelId)) &&
                    !enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.VredChannelId)))
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidAgeError), guid);
                    return validationErrors;
                }

                if (age < ConfigReader.Get<int>(Constants.Environment.VredCustomerAgeLimit) && enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.VredChannelId)))
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidCustomerAge), guid);
                    return validationErrors;
                }
            }

            if (enrolmentRequest.Data.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty && (enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.DeltaChannelID) || enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebMyaChannelId) || enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.AppMyaChannelId)))
            {
                var countryList = ConfigReader.Get(Constants.Environment.PostCodeMandatoryCountryList).Split("|");
                if (countryList.Contains(enrolmentRequest.Data.CustomerDetails.AddressDetails.FirstOrDefault().Country) && string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.AddressDetails.FirstOrDefault().PostalCode))
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), guid);
                    return validationErrors;
                }
            }

            return validationErrors;
        }

        public static List<ErrorDetails> ValidateBasicEnrolment(EnrolmentRequestCommand enrolmentRequest)
        {
            string guid = Guid.NewGuid().ToString();
            List<ErrorDetails> validationErrors = new();

            bool IsValidChannel = ValidateChannel.Validate(enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel);
            if (!IsValidChannel)
            {
                validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidChannel), guid);
                return validationErrors;
            }

            if (string.IsNullOrEmpty(enrolmentRequest?.Data?.CustomerDetails.FirstName) || string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.LastName) || enrolmentRequest.Data.CustomerDetails.ContactDetails == null || enrolmentRequest.Data.CustomerDetails.Preferences == null)
            {
                validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), guid);
            }

            if (enrolmentRequest.Data.CustomerDetails.ContactDetails != null && enrolmentRequest.Data.CustomerDetails.Preferences != null)
            {
                if (enrolmentRequest.Data.CustomerDetails.ContactDetails.EmailDetails == null || enrolmentRequest.Data.CustomerDetails.ContactDetails.EmailDetails.All(x => string.IsNullOrEmpty(x.EmailId)) || !enrolmentRequest.Data.CustomerDetails.Preferences.Any(x => x.PreferenceType == Preferences.PreferenceTypes.VAAMarketingPermissions))
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), guid);
                }
            }
            return validationErrors;
        }
    }
}

