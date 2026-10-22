using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class PreferencesUpdateRequest
    {

        public PreferencesUpdateRequest(List<Preferences> preferences = default)
        {
            Preferences = preferences;
        }

        public List<Preferences> Preferences { get; set; }


    }
}
