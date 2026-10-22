using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers.ManageAccount
{
    public class UpdateMarketingPreferenceRequestMapper
    {
        public static UpdatePreference GetUpdateMarketingPreference(TokenB2CResponse response, UpdateMarketingPreferenceRequest request, CustomerDocument customerDocument)
        {
            return new UpdatePreference()
            {
                FcNumber = response.FcNumber,
                CustomerId = customerDocument?.CustomerId,
                Preference = GetPreferences(request.PreferenceValue),
                EmailAddress = response.EmailAddress,
                FirstName = request.IsVacMember ? customerDocument?.FirstName : null,
                TierCode = request.IsVacMember ? Application.VirginAccountTierCode : null,
                LastUpdatedBy = Application.WebMyaChannelID
            };
        }

        public static IEnumerable<Preference> GetPreferences(bool preferenceValue)
        {
            return new Preference[]
            {
                new Preference()
                {
                    PreferenceType = Common.Constants.Application.PrefenceType,
                    PreferenceValue = preferenceValue ? "Y" : "N"
                }
            };
        }

    }
}
