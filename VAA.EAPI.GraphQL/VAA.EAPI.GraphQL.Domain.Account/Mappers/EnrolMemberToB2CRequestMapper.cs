using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using AccountPreference = VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS.AccountPreference;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class EnrolMemberToB2CRequestMapper
    {
        public static IdentityMsEnrolB2CRequest CreateEnrolMemberToB2CRequest(string memberNumber, string customerId, string status, CustomerAccountDetails customerDetails)
        {
            if (customerDetails == null) throw new ArgumentNullException(nameof(customerDetails));

            customerDetails.FcNumber = memberNumber;
            customerDetails.CustomerId = customerId;
            customerDetails.Status = status;

            if (!customerDetails.Preferences.Any(x => x.PreferenceType == AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions) && ConfigReader.Get(Constants.Application.VredMarketingFeatureFlag) == "Y")
            {
                customerDetails.Preferences.ToList().Add(new AccountPreference() { PreferenceType = AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions, PreferenceValue = string.Empty });
            }

            return new IdentityMsEnrolB2CRequest
            {
                CustomerDetails = customerDetails
            };
        }

        public static CustomerAccountDetails CreateEntraEnrolMemberToB2CRequest(string memberNumber, string customerId, string status, CustomerAccountDetails customerDetails, string enrolmentToken)
        {
            if (customerDetails == null) throw new ArgumentNullException(nameof(customerDetails));

            customerDetails.FcNumber = memberNumber;
            customerDetails.CustomerId = customerId;
            customerDetails.Status = status;
            customerDetails.ContinuationToken = enrolmentToken;

            var preferences = (customerDetails.Preferences ?? Enumerable.Empty<AccountPreference>()).ToList();

            if (!preferences.Any(x => x.PreferenceType == AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions)
                && ConfigReader.Get(Constants.Application.VredMarketingFeatureFlag) == "Y")
            {
                preferences.Add(new AccountPreference { PreferenceType = AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions, PreferenceValue = string.Empty });
                customerDetails.Preferences = preferences;
            }

            return customerDetails;
        }
    }
}

