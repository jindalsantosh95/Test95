using System;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment.CustomerAttribute;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    public class UpgradeMemberMapper
    {
        public static UpgradeVACMemberResponse MapToUpgradeVACMemberResponse(Exception exception, string responseMessage)
        {
            if (exception is not null)
            {
                return new UpgradeVACMemberResponse()
                {
                    Errors = new List<ErrorDetails>()
                    {
                         new ErrorDetails()
                         {
                             Message= exception.Message,
                             DeveloperMessage= exception.InnerException.Message
                         }
                    }
                };
            }

            return new UpgradeVACMemberResponse()
            {
                value = responseMessage
            };
        }

        public static UpgradeMemberMSRequest MapToUpgradeVACMemberRequest(UpgradeVACMemberRequest upgradeRequest)
        {
            if (upgradeRequest is null) throw new ArgumentNullException(nameof(upgradeRequest));
            if (upgradeRequest.CustomerDetails is null) throw new ArgumentNullException(nameof(upgradeRequest.CustomerDetails));

            List<Email> EmailDetailsList = new List<Email>();
            List<Phone> phoneDetailsList = new List<Phone>();
            List<UpgradeAddressRequest> addressList = new List<UpgradeAddressRequest>();
            List<AccountPreference> accountPreferenceList = new List<AccountPreference>();
            List<CustomerAttribute> customerAttributeList = new List<CustomerAttribute>();
            GuardianDetails guardianDetails = null;

            if (upgradeRequest.CustomerDetails.ContactDetails != null)
            {
                foreach (var emailDetail in upgradeRequest.CustomerDetails.ContactDetails.EmailDetails)
                {
                    EmailDetailsList.Add(new Email()
                    {
                        Usage = emailDetail.Usage?.ToString(),
                        emailId = emailDetail.EmailId ?? string.Empty,
                        PreferredFlag = emailDetail.PreferredFlag ?? string.Empty,
                        Description = emailDetail.Description ?? string.Empty
                    }
                    );
                }

                foreach (var emailDetail in upgradeRequest.CustomerDetails.ContactDetails.PhoneDetails)
                {
                    phoneDetailsList.Add(new Phone()
                    {
                        PhoneType = emailDetail.PhoneType?.ToString(),
                        Usage = emailDetail.Usage?.ToString(),
                        CountryCode = emailDetail.CountryCode ?? string.Empty,
                        PhoneNumber = emailDetail.PhoneNumber ?? string.Empty,
                        PreferredFlag = emailDetail.PreferredFlag ?? string.Empty,
                        Description = emailDetail.Description ?? string.Empty
                    }
                    );
                }
            }

            foreach (var addressDetail in upgradeRequest.CustomerDetails.AddressDetails)
            {
                addressList.Add(new UpgradeAddressRequest()
                {
                    AddressType = addressDetail.AddressType?.ToString() ?? string.Empty,
                    AddressLine1 = addressDetail.AddressLine1 ?? string.Empty,
                    AddressLine2 = addressDetail.AddressLine2 ?? string.Empty,
                    AddressLine3 = addressDetail.AddressLine3 ?? string.Empty,
                    CompanyName = addressDetail.CompanyName ?? string.Empty,
                    City = addressDetail.City ?? string.Empty,
                    PostalCode = addressDetail.PostalCode ?? string.Empty,
                    Region = addressDetail.Region ?? string.Empty,
                    CountryCode = addressDetail.Country ?? string.Empty,
                    PreferredFlag = addressDetail.PreferredFlag ?? string.Empty,
                    AddressVerified = Boolean.Parse(addressDetail.AddressVerified?.ToString()),
                    CountryName = addressDetail.CountryName ?? string.Empty,
                    MetroArea = addressDetail.MetroArea ?? string.Empty,
                    Valid = (!string.IsNullOrEmpty(addressDetail.PreferredFlag) && addressDetail.PreferredFlag.Is("Y")) ? "Y" : "N"
                }
                );
            }

            foreach (var addressDetail in upgradeRequest.CustomerDetails.Preferences)
            {
                accountPreferenceList.Add(new AccountPreference()
                {
                    PreferenceType = Enum.Parse<AccountPreference.AccountPreferenceTypes>(addressDetail.PreferenceType?.ToString()),
                    PreferenceValue = addressDetail.PreferenceValue ?? string.Empty
                }
                );
            }

            foreach (var attribute in upgradeRequest.CustomerDetails.CustomerAttribute)
            {
                if (!String.IsNullOrEmpty(attribute.Value) && !String.IsNullOrEmpty(attribute.Type?.ToString()))
                {
                    customerAttributeList.Add(new CustomerAttribute()
                    {
                        Value = attribute.Value ?? string.Empty,
                        Type = Enum.Parse<CustomerAttributeTypes>(attribute.Type?.ToString())
                    }
                    );
                }
            }

            if (upgradeRequest.CustomerDetails.LoyaltyMember.GuardianDetails != null)
            {
                guardianDetails = new GuardianDetails()
                {
                    Name = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails != null ? upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails.Name : null,
                    DateOfBirth = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails != null ? upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails.DateOfBirth : null,
                    CardNumber = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails != null ? upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails.CardNumber : null,
                    PhoneDetails = new()
                    {
                        Usage = Enum.Parse<PhoneDetail.PhoneUsages>(upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.PhoneDetails?.Usage?.ToString()),
                        CountryCode = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.PhoneDetails?.CountryCode,
                        PhoneNumber = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.PhoneDetails?.PhoneNumber,
                        PreferredFlag = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.PhoneDetails?.PreferredFlag,
                        Description = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.PhoneDetails?.Description,
                    },
                    EmailDetails = new()
                    {
                        Usage = Enum.Parse<EmailDetail.EmailUsages>(upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.EmailDetails?.Usage?.ToString()),
                        EmailId = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.EmailDetails?.EmailId,
                        PreferredFlag = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.EmailDetails?.PreferredFlag,
                        Description = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.EmailDetails?.Description,
                    },
                    GuardianTermsAndConditionsAccepted = upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails != null ? upgradeRequest.CustomerDetails.LoyaltyMember?.GuardianDetails?.GuardianTermsAndConditionsAccepted : null
                };
            }

            return new UpgradeMemberMSRequest
            {
                CustomerDetails = new()
                {
                    ChannelUserId = upgradeRequest.InitiatedBy,
                    FirstName = upgradeRequest.CustomerDetails.FirstName ?? string.Empty,
                    LastName = upgradeRequest.CustomerDetails.LastName ?? string.Empty,
                    MiddleName = upgradeRequest.CustomerDetails.MiddleName ?? string.Empty,
                    PreferredFirstName = upgradeRequest.CustomerDetails.PreferredFirstName ?? string.Empty,
                    PreferredLastName = upgradeRequest.CustomerDetails.PreferredLastName ?? string.Empty,
                    PreferredGender = upgradeRequest.CustomerDetails.PreferredGender ?? string.Empty,
                    PreferredName = upgradeRequest.CustomerDetails.PreferredName ?? string.Empty,
                    PreferredSalutation = upgradeRequest.CustomerDetails.PreferredSalutation ?? string.Empty,
                    PreferredPronouns = upgradeRequest.CustomerDetails.PreferredPronouns ?? string.Empty,
                    Gender = upgradeRequest.CustomerDetails.Gender ?? string.Empty,
                    Title = upgradeRequest.CustomerDetails.Title ?? string.Empty,
                    Salutation = upgradeRequest.CustomerDetails.Salutation ?? string.Empty,
                    Nationality = upgradeRequest.CustomerDetails.Nationality ?? string.Empty,
                    CustomerType = Enum.Parse<CustomerAccountTypeEnum>(upgradeRequest.CustomerDetails.CustomerType?.ToString()),
                    DateOfBirth = upgradeRequest.CustomerDetails.DateOfBirth,
                    Password = upgradeRequest.CustomerDetails.Password ?? string.Empty,
                    AddressDetails = addressList ?? new List<UpgradeAddressRequest>(),
                    TestMember = Convert.ToBoolean(upgradeRequest.CustomerDetails.LoyaltyMember?.TestMember),
                    ProgramName = upgradeRequest.CustomerDetails.LoyaltyMember?.ProgramName?.ToString() ?? string.Empty,
                    FlyingClubCardNumber = upgradeRequest.CustomerDetails.LoyaltyMember?.FlyingClubCardNumber ?? string.Empty,
                    CampaignCode = upgradeRequest.CustomerDetails.LoyaltyMember?.CampaignCode ?? string.Empty,
                    ContactDetails = new()
                    {
                        EmailDetails = EmailDetailsList,
                        PhoneDetails = phoneDetailsList
                    },
                    MemberType = upgradeRequest.CustomerDetails.LoyaltyMember?.MemberType?.ToString() ?? string.Empty,
                    EnrollmentChannel = upgradeRequest.CustomerDetails.LoyaltyMember?.EnrollmentChannel ?? string.Empty,
                    MediaCode = upgradeRequest.CustomerDetails.LoyaltyMember?.MediaCode ?? string.Empty,
                    Status = upgradeRequest.CustomerDetails.LoyaltyMember?.Status?.ToString() ?? string.Empty,
                    TermsAndConditionsAccepted = Convert.ToBoolean(upgradeRequest.CustomerDetails.LoyaltyMember?.TermsAndConditionsAccepted),
                    Preferences = accountPreferenceList ?? new List<AccountPreference>(),
                    GuardianDetails = guardianDetails,
                    PartnerMember = upgradeRequest.CustomerDetails.LoyaltyMember?.PartnerMember ?? new PartnerMember(),
                    CustomerAttribute = customerAttributeList ?? new List<CustomerAttribute>(),
                    FcNumber = upgradeRequest.CustomerDetails.LoyaltyMember?.MemberNumber ?? string.Empty,
                    CustomerId = upgradeRequest.CustomerId ?? string.Empty
                }
            };
        }
    }
}
