using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount
{
    public class UpdatePreferenceRequest
    {
        public IEnumerable<AccountPreference> Preferences { get; set; }
        public string UpdatedBy { get; set; }
    }
}
