using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class VredCardDetailsQueryProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public VredCardDetailsQueryProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new VredCardDetailsQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new VredCardDetailsQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task VredCardDetailsRetrievalShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<APIResponse<VredCardDetails>>("MemberCardLink\\VredCardDetails_APIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token

            _mockConnector.Setup(m => m.GetAsync<VredCardDetails>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            var provider = new VredCardDetailsQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);

            var queryResponse = await provider.GetVredCardDetails("9106913081");

            Assert.NotNull(queryResponse);
            Assert.Equal(queryResponse.PartnerUserId, response.Response.PartnerUserId);
        }
    }
}
