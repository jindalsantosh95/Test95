using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Trips.Constants;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL
{
    public class Content
    {
        public string env = Config.Environment;

        //This is the intital scaffolding for the content GQL which will need building out once the content fragments are available.
        [SkippableTheory]
        [Trait("Category", Traits.ContentValidationsMobileRegression)]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        [InlineData("vaa-sales-banners")]
        [InlineData("vaa-app-destinations")]
        [InlineData("vaa-app-cabin-overview")]
        public async Task Content_GQL_Success(string contentFragmentId)
        {

            HttpResponseMessage response = GraphQlEndpoint.Content(env, contentFragmentId).EnsureGQLResponseSuccess();

            dynamic dynamicResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            // Validate data exists
            Assert.True(DynamicHelper.HasProperty(dynamicResponse, "data"), "Response should contain data property");

            Assert.True(DynamicHelper.HasProperty(dynamicResponse, "GetContentFragment"), "Response should contain GetContentFragment property");
        }
    }
}
