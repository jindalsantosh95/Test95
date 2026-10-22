using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Channel;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using static VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails.Customer;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Validations
{
    public static class ValidatePayload
    {
        public static List<ErrorDetails> Validate(EnrolmentRequest enrolmentRequest, string channelId, List<string> blackListedEmails)
        {
            string guid = Guid.NewGuid().ToString();
            List<Channel> channeldetails = Helper.ChannelInformation.GetchannelDetails();
            List<ErrorDetails> validationErrors = new List<ErrorDetails>();

            bool IsValidChannel = ValidateChannel.Validate(channelId);
            if (!IsValidChannel)
            {
                validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidChannelError), guid);
                return validationErrors;
            }

            if (ConfigReader.Get(Constants.Environment.PowerAppChannelID) == channelId && string.IsNullOrEmpty(enrolmentRequest.ChannelUserId))
            {
                validationErrors = Helper.ErrorMapperHelper.MapValidationError(Constants.ErrorCodes.MissingChanneluserIDError, guid);
                return validationErrors;
            }
            else if (ConfigReader.Get(Constants.Environment.PowerAppChannelID) == channelId && enrolmentRequest.CustomerDetails.AddressDetails != null && enrolmentRequest.CustomerDetails.AddressDetails.Any(a => string.IsNullOrEmpty(a.AddressVerified.ToString())) && enrolmentRequest.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty)
            {
                validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), guid);
                return validationErrors;
            }

            if (enrolmentRequest.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty && enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                if (enrolmentRequest.CustomerDetails.LoyaltyMember == null || enrolmentRequest.CustomerDetails.LoyaltyMember.MemberType == null || Convert.ToString(enrolmentRequest.CustomerDetails.LoyaltyMember.MemberType.Value) != Domain.CustomerDetails.LoyaltyMember.MemberTypes.Individual.ToString())
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidMemberType), guid);
                    return validationErrors;
                }

            }

            bool isEligibleMemberType = ValidateChannel.Validate(channelId);
            if (!isEligibleMemberType)
            {
                validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidChannelError), guid);
                return validationErrors;
            }

            if (enrolmentRequest.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty)
            {
                bool isValidCustomerDob = Helper.CheckDateFormat.IsValidDate(enrolmentRequest.CustomerDetails.DateOfBirth);
                if (!isValidCustomerDob)
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidDateFromat), guid);
                    return validationErrors;
                }
            }

            if (enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails != null && enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.DateOfBirth != null && enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.VredChannelId))
            {

                bool isValidCustomerGuardianDob = Helper.CheckDateFormat.IsValidDate(enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails.DateOfBirth);
                if (!isValidCustomerGuardianDob)
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidDateFromat), guid);
                    return validationErrors;
                }

            }
            if (channeldetails.Where(c => c.Code == enrolmentRequest?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateCustomerAge)
            {
                int customertAgelimit = ConfigReader.Get<int>(Constants.Environment.CustomerAgeLimit);
                string customerDOB = enrolmentRequest.CustomerDetails.DateOfBirth;
                bool IsValidAge = ValidateAge.Validate(customerDOB, customertAgelimit);
                if (!IsValidAge)
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerAge), guid);
                    return validationErrors;
                }
            }
            if (enrolmentRequest.CustomerDetails.LoyaltyMember?.GuardianDetails != null && enrolmentRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.DateOfBirth != null && enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                int parentAgelimit = ConfigReader.Get<int>(Constants.Environment.ParentAgeLimit);
                string parentDOB = enrolmentRequest.CustomerDetails.LoyaltyMember.GuardianDetails.DateOfBirth;
                bool IsParentValidAge = ValidateAge.Validate(parentDOB, parentAgelimit);
                if (!IsParentValidAge)
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidParentAge), guid);
                    return validationErrors;
                }
            }
            if ((enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.DeltaChannelID)) || enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.WebMyaChannelId))) && enrolmentRequest.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty)
            {

                if (enrolmentRequest.CustomerDetails.AddressDetails.Any(a => string.IsNullOrEmpty(a.AddressVerified.ToString())))
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), guid);
                    return validationErrors;
                }
            }

            if (enrolmentRequest.CustomerDetails.ContactDetails != null && enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails != null && channeldetails.Where(c => c.Code == enrolmentRequest?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateEmail)
            {
                foreach (Domain.CustomerDetails.Email email in enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails)
                {
                    bool IsValidEmailId = ValidateEmail.Validate(email.EmailId);
                    if (!IsValidEmailId)
                    {
                        validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidEmailFromat), guid);
                        return validationErrors;
                    }
                }

                if (enrolmentRequest.CustomerDetails.LoyaltyMember != null && enrolmentRequest.CustomerDetails.LoyaltyMember.GuardianDetails != null &&
                  enrolmentRequest.CustomerDetails.LoyaltyMember.GuardianDetails.EmailDetails != null &&
                  enrolmentRequest.CustomerDetails.LoyaltyMember.GuardianDetails.EmailDetails.EmailId != null &&
                  channeldetails.Where(c => c.Code == enrolmentRequest?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateEmail)
                {
                    bool IsValidEmailId = ValidateEmail.Validate(enrolmentRequest.CustomerDetails.LoyaltyMember.GuardianDetails.EmailDetails.EmailId);
                    if (!IsValidEmailId)
                    {
                        validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidEmailFromat), guid);
                        return validationErrors;
                    }
                }
            }

            if (enrolmentRequest.CustomerDetails.ContactDetails != null && enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails != null)
            {
                foreach (Domain.CustomerDetails.Email email in enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails)
                {
                    if (blackListedEmails.Any(x => x.Is(email.EmailId)))
                    {
                        validationErrors = Helper.ErrorMapperHelper.MapValidationError(Constants.ErrorCodes.BlacklistedEmailidError, guid);

                        return validationErrors;
                    }
                }
            }

            if (enrolmentRequest.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty)
            {
                int age = DateTime.Now.Year - Convert.ToDateTime(enrolmentRequest?.CustomerDetails?.DateOfBirth).Year;
                bool GuardianTerms = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.GuardianTermsAndConditionsAccepted ?? false;

                if (age < ConfigReader.Get<int>(Constants.Environment.EnrolmentAgeLimit) && GuardianTerms && !enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.DeltaChannelID)) && !enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.WebMyaChannelId)) && !enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.PowerAppChannelID)) ||
                   age < ConfigReader.Get<int>(Constants.Environment.EnrolmentAgeLimit) && !GuardianTerms && !enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.DeltaChannelID)) && !enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.WebMyaChannelId)) &&
                    !enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.VredChannelId)))
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidAgeError), guid);
                    return validationErrors;
                }

                if (age < ConfigReader.Get<int>(Constants.Environment.VredCustomerAgeLimit) && enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel.Equals(ConfigReader.Get(Constants.Environment.VredChannelId)))
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerAge), guid);
                    return validationErrors;
                }
            }

            if (enrolmentRequest.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty && (enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.DeltaChannelID) || enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebMyaChannelId)))
            {
                var countryList = ConfigReader.Get(Constants.Environment.PostCodeMandatoryCountryList).Split("|");
                if (countryList.Contains(enrolmentRequest.CustomerDetails.AddressDetails.FirstOrDefault().Country) && string.IsNullOrEmpty(enrolmentRequest.CustomerDetails.AddressDetails.FirstOrDefault().PostalCode))
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), guid);
                    return validationErrors;
                }
            }

            return validationErrors;
        }

        public static List<ErrorDetails> ValidateBasicEnrolment(EnrolmentRequest enrolmentRequest)
        {
            string guid = Guid.NewGuid().ToString();
            List<ErrorDetails> validationErrors = new();

            bool IsValidChannel = ValidateChannel.Validate(enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel);
            if (!IsValidChannel)
            {
                validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidChannel), guid);
                return validationErrors;
            }

            if (string.IsNullOrEmpty(enrolmentRequest.CustomerDetails.FirstName) || string.IsNullOrEmpty(enrolmentRequest.CustomerDetails.LastName) || enrolmentRequest.CustomerDetails.ContactDetails == null || enrolmentRequest.CustomerDetails.Preferences == null)
            {
                validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), guid);
            }

            if (enrolmentRequest.CustomerDetails.ContactDetails != null && enrolmentRequest.CustomerDetails.Preferences != null)
            {
                if (enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails == null || enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails.All(x => string.IsNullOrEmpty(x.EmailId)) || !enrolmentRequest.CustomerDetails.Preferences.Any(x => x.PreferenceType == Domain.CustomerDetails.Preferences.PreferenceTypes.VAAMarketingPermissions))
                {
                    validationErrors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), guid);
                }
            }
            return validationErrors;
        }
    }
}

