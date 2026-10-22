using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class EnrolmentEventDataMapper
    {
        public static object CreateEnrolmentEventDataMapper(CreateCustomerAccountRequest request)
        {

            return new
            {
                DeDupRequest = new
                {
                    Id = request.ChannelUserId,
                    Data = new { CustomerDetails = request?.CustomerDetails }
                }
            };
        }

        public static EnrolmentRequest CreateEnrolmentDataMapper(CustomerCustMS customer, VirginAccountDetails virginAccountDetails, IEnumerable<CountryDetails> countryDetails)
        {

            var addressDetails = new List<UpgradeAddressRequest>
            {
                new()
                {
                    AddressLine1 = virginAccountDetails.AddressDetails.AddressLine1,
                    AddressLine2 = virginAccountDetails.AddressDetails.AddressLine2,
                    AddressLine3 = virginAccountDetails.AddressDetails.AddressLine3,
                    AddressType = virginAccountDetails.AddressDetails.AddressType.ToString(),
                    City = virginAccountDetails.AddressDetails.City,
                    CompanyName = virginAccountDetails.AddressDetails.CompanyName,
                    Country = virginAccountDetails.AddressDetails.Country,
                    CountryName = countryDetails.First(x => x.Name == virginAccountDetails.AddressDetails.Country).value,
                    CountryCode =  virginAccountDetails.AddressDetails.Country,
                    PostalCode = virginAccountDetails.AddressDetails.PostalCode,
                    PreferredFlag = virginAccountDetails.AddressDetails.PreferredFlag,
                    Region = virginAccountDetails.AddressDetails.Region,
                    MetroArea = countryDetails.First(m => m.type.Is("COUNTRY") && m.Name == virginAccountDetails.AddressDetails.Country)?.metroArea,
                    Valid = virginAccountDetails.AddressDetails.PreferredFlag.Is("Y") ? "Y" : "N",
                    AddressVerified = virginAccountDetails.AddressDetails.PreferredFlag.Is("Y") ? true : false
                }
            };

            EnrolmentRequest enrolmentRequest = new()
            {
                CustomerDetails = new()
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    DateOfBirth = customer.DateOfBirth.ToString(Common.Constants.Application.ISODateFormat),
                    Title = customer.Title,
                    CustomerId = customer.CustomerId,
                    AddressDetails = addressDetails,
                    ContactDetails = new Contact() { EmailDetails = new(), PhoneDetails = new() },
                    Gender = virginAccountDetails.Gender,
                    Salutation = customer.Salutation,
                    Nationality = customer.Nationality,
                    Preferences = virginAccountDetails.Preferences,
                    MediaCode = Common.Constants.Application.MediaCode,
                    FcNumber = customer.LoyaltyNumber,
                    CustomerType = CustomerAccountTypeEnum.Loyalty,
                    EnrollmentChannel = virginAccountDetails.EnrolmentChannel == null ? Common.Constants.Application.WebMyaChannelID : virginAccountDetails.EnrolmentChannel,
                    MemberType = Constants.Application.IndividualAccount,
                    TermsAndConditionsAccepted = virginAccountDetails.TermsAndConditionsAccepted,
                    TestMember = false,
                    Status = customer.Status
                }
            };

            if (customer.ContactDetails?.EmailDetails != null)
            {
                customer.ContactDetails?.EmailDetails.ForEach(item =>
                {
                    enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails.Add(new Email()
                    {
                        emailId = item.emailId.ToLower(),
                        Description = item.Description,
                        Usage = AccountEmailDetail.AccountEmailUsages.Personal.ToString(),
                        PreferredFlag = item.PreferredFlag
                    });

                });
            }

            if (customer.ContactDetails?.PhoneDetails != null)
            {
                customer.ContactDetails?.PhoneDetails.ForEach(item =>
                {
                    enrolmentRequest.CustomerDetails.ContactDetails.PhoneDetails.Add(new Phone()
                    {
                        PhoneNumber = item.PhoneNumber,
                        PhoneType = AccountPhoneDetail.AccountPhoneTypes.Mobile.ToString(),
                        Usage = AccountPhoneDetail.AccountPhoneUsages.Personal.ToString(),
                        Description = item.Description,
                        PreferredFlag = item.PreferredFlag,
                        CountryCode = item.CountryCode
                    });
                });
            }

            return enrolmentRequest;
        }
    }
}

