using VAA.EAPI.Automation.Core.Constants;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using VAA.EAPI.Automation.Heimdall.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.AzureB2C
{
    public class OpenIdConfiguration
    {
        [Trait("Category", GlobalTraits.HealthCheck)]
        [Trait("Tag", GlobalTraits.MyAccount)]
        [Theory]
        [InlineData("SignUpSignIn")]
        [InlineData("WellKnown")]
        [InlineData("WellknownV2")]
        public void OpenIdConfigurationUrls(string endpoint)
        {
            OpenIdConfigurationEndpoints.OpenIdConfiguration(endpoint).EnsureIsSuccessStatusCode();
        }
    }
}