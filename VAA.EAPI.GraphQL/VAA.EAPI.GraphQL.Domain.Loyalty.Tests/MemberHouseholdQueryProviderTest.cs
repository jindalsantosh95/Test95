using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class MemberHouseholdQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mockTokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        public MemberHouseholdQueryProviderTest()
        {
            _mockTokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void ConstructorShouldThrowExceptionWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberHouseholdQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new MemberHouseholdQueryProvider(_mockTokenProvider.Object, null));
        }
        [Fact]
        public async Task QueryShouldSucceed()
        {
            APIResponse<MemberHouseholdResponse> response = TestDataProvider.GetTestData<APIResponse<MemberHouseholdResponse>>("Loyalty\\MemberHousehold");
            _mockTokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.GetAsync<MemberHouseholdResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);
            var provider = new MemberHouseholdQueryProvider(_mockTokenProvider.Object, _mockConnector.Object);
            var customerId = "D4718748-3560-E0ED-E053-6941D90AAD4B";
            var queryResponse = await provider.Query(customerId);
            Assert.IsType<MemberHouseholdResponse>(queryResponse);
            Assert.All(queryResponse.HouseholdMembers, householdMember => Assert.Equal("Linked Member", householdMember.RelationshipType));
        }
    }
}

