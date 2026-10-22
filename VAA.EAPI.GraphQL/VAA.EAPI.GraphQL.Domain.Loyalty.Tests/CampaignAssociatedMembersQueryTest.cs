using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CampaignAssociatedMembersQueryTest
    {
        private readonly Mock<ILogger<CampaignAssociatedMembersQuery>> _logger;
        private readonly CampaignAssociatedMembersQuery _assocMembersQuery;
        private readonly Mock<IAssociatedMembersQueryProvider> _provider;

        public CampaignAssociatedMembersQueryTest()
        {
            _logger = new Mock<ILogger<CampaignAssociatedMembersQuery>>();
            _assocMembersQuery = new CampaignAssociatedMembersQuery(_logger.Object);
            _provider = new Mock<IAssociatedMembersQueryProvider>();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignAssociatedMembersQuery(null));
        }

        [Fact]
        public async Task CampaignAssociatedMembersQueryShouldSucceed()
        {
            AssociatedMembersDownloadResponse response = new() { AssociatedMembersFileURL = "dummyurl.com" };

            _provider.Setup(m => m.Query(It.IsAny<string>())).ReturnsAsync(response);

            var queryResponse = await _assocMembersQuery.AssociatedMembers("XYZ", _provider.Object, _logger.Object);
            Assert.IsType<AssociatedMembersDownloadResponse>(queryResponse);
            Assert.NotNull(response.AssociatedMembersFileURL);
        }

        [Fact]
        public async Task CampaignAssociatedMembersQueryShouldThrowException()
        {
            _provider.Setup(m => m.Query(It.IsAny<string>())).Throws(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _assocMembersQuery.AssociatedMembers("XYZ", _provider.Object, _logger.Object));
        }

        [Fact]
        public async Task CampaignAssociatedMembersQueryShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _assocMembersQuery.AssociatedMembers("XYZ", null, _logger.Object));
            await Assert.ThrowsAsync<ArgumentException>(() => _assocMembersQuery.AssociatedMembers(null, _provider.Object, _logger.Object));
        }
    }
}
