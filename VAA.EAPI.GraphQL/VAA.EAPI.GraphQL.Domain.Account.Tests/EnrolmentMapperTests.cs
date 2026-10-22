using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class EnrolmentMapperTests
    {
        public EnrolmentMapperTests()
        {
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CreateEnrolmentEventDataMapper_ShouldMapChannelUserIdAsDeDupId()
        {
            CreateCustomerAccountRequest request = new()
            {
                ChannelUserId = "123456",
                EnrolmentToken = "signup-continuation-token",
                CustomerDetails = new CustomerAccountDetails
                {
                    FirstName = "eapitestingaaae",
                    LastName = "eapitestingbfdaaaabcde"
                }
            };

            var mappedRequest = EnrolmentEventDataMapper.CreateEnrolmentEventDataMapper(request);
            var json = JObject.Parse(Json.Serialize(mappedRequest));

            Assert.Equal("123456", json.SelectToken("deDupRequest.id")?.Value<string>());
            Assert.Null(json.SelectToken("deDupRequest.channelUserId"));
            Assert.Equal("eapitestingaaae", json.SelectToken("deDupRequest.data.customerDetails.firstName")?.Value<string>());
            Assert.Null(json.SelectToken("deDupRequest.enrolmentToken"));
            Assert.Null(json.SelectToken("deDupRequest.data.customerDetails.continuationToken"));
        }

        [Fact]
        public void CreateEntraEnrolMemberToB2CRequest_ShouldUseEnrolmentTokenAsContinuationToken()
        {
            CustomerAccountDetails customerDetails = new()
            {
                Preferences = new List<AccountPreference>()
            };

            var mappedRequest = EnrolMemberToB2CRequestMapper.CreateEntraEnrolMemberToB2CRequest(
                "1501033656",
                "0078d8f1-e7dd-46a5-9c2f-7006b4fd0fc9",
                "Active",
                customerDetails,
                "signup-continuation-token");

            Assert.Same(customerDetails, mappedRequest);
            Assert.Equal("1501033656", mappedRequest.FcNumber);
            Assert.Equal("0078d8f1-e7dd-46a5-9c2f-7006b4fd0fc9", mappedRequest.CustomerId);
            Assert.Equal("Active", mappedRequest.Status);
            Assert.Equal("signup-continuation-token", mappedRequest.ContinuationToken);
        }
    }
}
