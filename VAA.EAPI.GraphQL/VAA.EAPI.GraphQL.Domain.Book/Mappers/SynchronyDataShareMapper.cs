using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment.CoBrand;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Book.Mappers
{
    public class SynchronyDataShareMapper
    {
        public static SynchronyTokenMSRequest GetSynchronyTokenRequest(CustomerDTO customer, string engagementScore, Basket basket, double initialPurchaseAmount)
        {
            var customerInfo = customer?.Customer;
            var contactDetails = customerInfo?.ContactDetails;
            var basketCustomerInfo = basket?.People?.FirstOrDefault(x => x.Id == "0");
            var basketContactDetails = basket?.ContactDetails;

            return new SynchronyTokenMSRequest
            {
                AccountInfo = engagementScore,
                ApplicantDetails = new()
                {
                    FirstName = customerInfo?.FirstName ?? basketCustomerInfo?.FirstName,
                    LastName = customerInfo?.LastName ?? basketCustomerInfo?.LastName,
                    DateOfBirth = customerInfo?.DateOfBirth.ToString("yyyy-MM-dd") ?? basketCustomerInfo?.DateOfBirth?.ToString("yyyy-MM-dd"),
                    Contacts = new()
                    {
                        Email = contactDetails?.EmailDetails?.FirstOrDefault(c => c.PreferredFlag == "Y")?.emailId ?? basketContactDetails?.EmailAddress,
                        Phone = contactDetails?.PhoneDetails?.FirstOrDefault(p => p.PreferredFlag == "Y")?.PhoneNumber ?? basketContactDetails?.PhoneNumber,
                        Countrycode = contactDetails?.PhoneDetails?.FirstOrDefault(p => p.PreferredFlag == "Y")?.CountryCode ?? basketContactDetails?.CountryCode
                    },
                    Addresses = customerInfo?.AddressDetails?.FirstOrDefault(a => a.PreferredFlag == "Y") == null ? null : new() { PostalCode = customerInfo.AddressDetails?.FirstOrDefault(a => a.PreferredFlag == "Y")?.PostalCode },
                    LoyaltyMemberDetails = new() { MemberNumber = customerInfo?.LoyaltyNumber ?? (basketCustomerInfo.MembershipDetails?.FQTProgramme == "VS" ? basketCustomerInfo.MembershipDetails?.FQTNumber : null) }
                },
                InitialPurchaseAmount = initialPurchaseAmount.ToString()
            };
        }
    }
}
