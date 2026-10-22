using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Requests.Endpoints;

namespace VAA.EAPI.Automation.OCI.Tests.GraphQL
{
    public class CheckInReferenceGraphQL
    {
        [Theory]
        [Trait("Category", Traits.CheckInGraphQLCiCd)]
        [InlineData("GetGenderDetails")]
        [InlineData("GetStatesOfUSA")]
        [InlineData("GetNationalityDetails")]
        [InlineData("GetPassportCountries")]
        public async Task ReferenceGraphQLTests(string referenceCall)
        {
            string referenceGraphQLRequest = GraphQLRequestBuilder.ReferenceGraphQLRequest(referenceCall);

            HttpResponseMessage referenceGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(referenceGraphQLRequest).EnsureGQLResponseSuccess();

            JToken responseJson = JsonConvert.DeserializeObject<JToken>(await referenceGraphQLResponse.Content.ReadAsStringAsync());

            JToken dataToken = responseJson.SelectToken($"$.data.{referenceCall}");

            Assert.NotNull(dataToken);

            Assert.True(dataToken.HasValues, $"Expected data for '{referenceCall}', but got none.");
        }
    }
}
