using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PartnerMembershipDocQueryProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public PartnerMembershipDocQueryProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipDocQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipDocQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task PartnerMembershipDetailsRetrievalShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<APIResponse<PartnerMembershipResponse>>("MemberCardLink\\PartnerMembership_APIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token

            _mockConnector.Setup(m => m.GetAsync<PartnerMembershipResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            var provider = new PartnerMembershipDocQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.GetPartnerMembershipDoc("D971F4D0-3608-84D1-E053-6941D90A4EF2");

            Assert.NotNull(queryResponse);
            Assert.Equal(queryResponse.CustomerId, response.Response.CustomerId);
        }
    }
}

