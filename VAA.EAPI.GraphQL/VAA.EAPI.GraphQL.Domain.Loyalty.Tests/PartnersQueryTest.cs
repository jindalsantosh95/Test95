using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PartnersQueryTest
    {
        private readonly Mock<ILogger<PartnersQuery>> _mockLogger;
        private readonly PartnersQuery _partnersQuery;
        private readonly Mock<IPartnersQueryProvider> _provider;

        public PartnersQueryTest()
        {
            _mockLogger = new Mock<ILogger<PartnersQuery>>();
            _partnersQuery = new PartnersQuery(_mockLogger.Object);
            _provider = new Mock<IPartnersQueryProvider>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PartnersQuery(null));
        }

        [Fact]
        public async Task PartnersRetrieveShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<List<PartnerProfile>>("PartnersRetrieval\\PartnersResponse");
            _provider.Setup(m => m.Query()).ReturnsAsync(response);
            var queryResponse = await _partnersQuery.Partners(_provider.Object, _mockLogger.Object);
            Assert.IsType<List<PartnerProfile>>(queryResponse);
            Assert.True(response.Count == queryResponse.Count);
        }

        [Fact]
        public async Task PartnersRetrieveShouldThrowException()
        {
            var response = TestDataProvider.GetTestData<List<PartnerProfile>>("PartnersRetrieval\\PartnersResponse");
            _provider.Setup(m => m.Query()).Throws(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _partnersQuery.Partners(_provider.Object, _mockLogger.Object));
        }

        [Fact]
        public async Task PartnersRetrieveShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _partnersQuery.Partners(null, _mockLogger.Object));
        }
    }
}
