using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Search.Constants;
using VAA.EAPI.Automation.Search.Requests.Endpoints;
using OciTraits = VAA.EAPI.Automation.OCI.Constants.Traits;

namespace VAA.EAPI.Automation.OCI.Tests.Microservices
{
    public class CheckInReferenceMSTests
    {
        [Theory]
        [Trait("Category", Traits.ReferenceMicroserviceCiCd)]
        [Trait("Category", OciTraits.SharedServicesSignOff)]
        [Trait("SharedService", "ReferenceMS")]
        [Trait("SharedServiceConsumer", "OCI")]
        [InlineData("GenderDetails")]
        [InlineData("StatesOfUSADetails")]
        [InlineData("NationalityDetails")]
        [InlineData("PassportCountryDetails")]
        public async Task ReferenceMS_Get_Tests(string referenceCall)
        {
            // calling reference countries get end point
            HttpResponseMessage referenceMSGet = MicroserviceEndpoint.ReferenceMSGet(referenceCall).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await referenceMSGet.Content.ReadAsStringAsync());

            Assert.True(result.Count > 0, $"No {referenceCall} details returned");
        }
    }
}
