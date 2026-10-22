using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PNR;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PNRQueryProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly PNRQueryProvider _provider;

        public PNRQueryProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _provider = new PNRQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PNRQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new PNRQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task RetrievePNR_SuccessAsync()
        {
            var retrievePNRResponse = TestDataProvider.GetTestData<APIResponse<RetrievePNRResponse>>("Loyalty//PNRAndFareCalc//RetrievePNRResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");
            _mockConnector.Setup(m => m.GetAsync<RetrievePNRResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(retrievePNRResponse);

            var queryResponse = await _provider.RetrievePNR("xxxxxx");

            Assert.NotNull(queryResponse);
            Assert.Equal("LHR", queryResponse.PnrType.SegmentDetails.FirstOrDefault().DeparturePoint);
            Assert.Equal("DEL", queryResponse.PnrType.SegmentDetails.FirstOrDefault().ArrivalPoint);
            Assert.Equal("9322327373234", queryResponse.PnrType.TicketInformation.FirstOrDefault().TicketNumber);
            Assert.Equal("SUNESH", queryResponse.PnrType.Passengers.FirstOrDefault().FirstName);
        }
    }
}
