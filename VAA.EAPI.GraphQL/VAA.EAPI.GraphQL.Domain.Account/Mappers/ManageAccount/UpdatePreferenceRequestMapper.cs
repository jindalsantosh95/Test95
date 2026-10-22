using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers.ManageAccount
{
    public class UpdatePreferenceRequestMapper
    {
        public static UpdatePreference GetUpdatedPreference(TokenB2CResponse response, UpdatePreferenceRequest request, CustomerDocument customerDocument)
        {
            return new UpdatePreference()
            {
                FcNumber = response.FcNumber,
                CustomerId = customerDocument?.CustomerId,
                Preference = request.Preferences.Select(p => new Preference() { PreferenceType = p.PreferenceType.ToString(), PreferenceValue = p.PreferenceValue }),
                EmailAddress = response.EmailAddress,
                LastUpdatedBy = request.UpdatedBy
            };
        }
    }
}
