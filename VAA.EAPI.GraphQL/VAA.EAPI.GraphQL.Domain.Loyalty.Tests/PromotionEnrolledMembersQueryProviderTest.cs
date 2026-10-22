using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PromotionEnrolledMembersQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public PromotionEnrolledMembersQueryProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new PromotionEnrolledMembersQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new PromotionEnrolledMembersQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task QueryShouldSucceed()
        {
            APIResponse<PromotionEnrolledMemberResponse> response = TestDataProvider.GetTestData<APIResponse<PromotionEnrolledMemberResponse>>("Loyalty\\PromotionEnrolledMembers");
            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.GetAsync<PromotionEnrolledMemberResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            var provider = new PromotionEnrolledMembersQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.Query("1-19J90XO");
            Assert.IsType<PromotionEnrolledMemberResponse>(queryResponse);
        }
    }
}
