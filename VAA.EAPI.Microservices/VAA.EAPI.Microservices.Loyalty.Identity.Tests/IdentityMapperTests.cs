using Shouldly;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class IdentityMapperTests
    {
        public IdentityMapperTests()
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void IdentityMapperSuccess()
        {
            CreateCustomerEntities createCustomerEntities = new CreateCustomerEntities()
            {
                AddressDetails = new System.Collections.Generic.List<AddressDetail>()
                {
                    new AddressDetail()
                    {
                         AddressLine1 = "1236 Test address1",
                         AddressLine2= "Home Locality",
                         PostalCode= "RH11 7NR",
                         CountryCode= "GB",
                         CountryName= "United Kingdom",
                         City = "Colchester",
                         AddressType = "Home",
                         PreferredFlag= "Y",
                         Region = "Essex"
                    }
                },
                ContactDetails = new ContactDetails()
                {
                    EmailDetails = new List<EmailDetail>()
                    {
                            new EmailDetail()
                            {
                                 EmailId = "testvirgin3473@noreply.com",
                                 Usage = "Business",
                                 PreferredFlag = "Y"
                            }
                    },
                    PhoneDetails = new List<PhoneDetail>()
                    {
                         new PhoneDetail()
                         {
                             PhoneNumber = "9999999999",
                             CountryCode = "44",
                             PhoneType = "Mobile"
                         }
                    }
                },
                Title = "Mr",
                FirstName = "testVRedEAPI",
                LastName = "LastvRedEAPI",
                DateOfBirth = "1990-12-15",
                Gender = "M",
                CustomerType = "Loyalty",
                SecurityQuestions = null,
                CustomerId = "43fa75ec-20dc-4a80-85d1-cf18cbddf470",
                FcNumber = "1075721520",
                EnrollmentChannel = "POWERAPP",
                MemberType = "Individual",
                Status = "Active"
            };
            IdentityRepoCommand identityRepoQuery = new IdentityRepoCommand(createCustomerEntities) { CreateCustomerEntities = createCustomerEntities };
            MemberEnrolmentGraphRequest request = IdentityRequestMapper.MapIdentityrequest(identityRepoQuery, identityRepoQuery.IsAccountEnabled);
            request.GivenName.ShouldBe("testVRedEAPI");
            request.Surname.ShouldBe("LastvRedEAPI");
        }
        [Fact]
        public void IdentityMapperFailure()
        {
            CreateCustomerEntities createCustomerEntities = new CreateCustomerEntities()
            {
                AddressDetails = new System.Collections.Generic.List<AddressDetail>()
                {
                    new AddressDetail()
                    {
                         AddressLine1 = "1236 Test address1",
                         AddressLine2= "Home Locality",
                         PostalCode= "RH11 7NR",
                         CountryCode= "GB",
                         CountryName= "United Kingdom",
                         City = "Colchester",
                         AddressType = "Home",
                         PreferredFlag= "Y",
                         Region = "Essex"
                    }
                },
                ContactDetails = new ContactDetails()
                {
                    EmailDetails = new List<EmailDetail>()
                    {
                            new EmailDetail()
                            {
                                 EmailId = "testvirgin3473@noreply.com",
                                 Usage = "Business",
                                 PreferredFlag = "Y"
                            }
                    },
                    PhoneDetails = new List<PhoneDetail>()
                    {
                         new PhoneDetail()
                         {
                             PhoneNumber = "9999999999",
                             CountryCode = "44",
                             PhoneType = "Mobile"
                         }
                    }
                },
                Title = "Mr",
                FirstName = "testVRedEAPI1",
                LastName = "LastvRedEAPI",
                DateOfBirth = "1990-12-15",
                Gender = "M",
                CustomerType = "Loyalty",
                SecurityQuestions = null,
                CustomerId = "43fa75ec-20dc-4a80-85d1-cf18cbddf470",
                FcNumber = "1075721520",
                EnrollmentChannel = "POWERAPP",
                MemberType = "Individual",
                Status = "Active"
            };
            IdentityRepoCommand identityRepoQuery = new IdentityRepoCommand(createCustomerEntities) { CreateCustomerEntities = createCustomerEntities };
            MemberEnrolmentGraphRequest request = IdentityRequestMapper.MapIdentityrequest(identityRepoQuery, identityRepoQuery.IsAccountEnabled);
            request.GivenName.ShouldNotBe("testVRedEAPI");
            request.Surname.ShouldBe("LastvRedEAPI");
        }

        [Fact]
        public void CustomerDetailsMapper_PreservesGuardianDetails()
        {
            var guardian = new AccountGuardianDetails
            {
                Name = "Parent Example",
                DateOfBirth = "1980-01-01",
                CardNumber = "1234567890",
                GuardianTermsAndConditionsAccepted = true,
                PhoneDetails = new AccountPhoneDetail
                {
                    PhoneType = AccountPhoneDetail.AccountPhoneTypes.Mobile,
                    Usage = AccountPhoneDetail.AccountPhoneUsages.Personal,
                    CountryCode = "44",
                    PhoneNumber = "7700900123",
                    PreferredFlag = "Y"
                },
                EmailDetails = new AccountEmailDetail
                {
                    Usage = AccountEmailDetail.AccountEmailUsages.Personal,
                    EmailId = "parent@example.com",
                    PreferredFlag = "Y",
                    Valid = "Y"
                }
            };
            var customerDetails = new CustomerAccountDetails
            {
                CustomerType = CustomerAccountTypeEnum.Loyalty,
                Status = "Active",
                LoyaltyMember = new AccountLoyaltyMember
                {
                    EnrollmentChannel = "WEB-MYA",
                    GuardianDetails = guardian
                }
            };

            var mapped = MapCustomerDetailsToCustomerEntities.MaptoCreateCustomerEntities(customerDetails);

            mapped.GuardianDetails.ShouldNotBeNull();
            mapped.GuardianDetails.Name.ShouldBe("Parent Example");
            mapped.GuardianDetails.DateOfBirth.ShouldBe("1980-01-01");
            mapped.GuardianDetails.CardNumber.ShouldBe("1234567890");
            mapped.GuardianDetails.GuardianTermsAndConditionsAccepted.ShouldBe(true);
            mapped.GuardianDetails.PhoneDetails.PhoneNumber.ShouldBe("7700900123");
            mapped.GuardianDetails.EmailDetails.EmailId.ShouldBe("parent@example.com");
        }
    }
}
