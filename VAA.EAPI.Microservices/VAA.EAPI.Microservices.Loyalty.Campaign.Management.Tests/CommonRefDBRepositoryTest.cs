using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CommonRefDBRepositoryTest
    {
        private readonly Mock<ICommonRefDBServices> _commonRefDBServices;
        private readonly Mock<ILogger<CommonRefDBRepository>> _logger;
        private readonly CommonRefDBRepository _repo;
        public CommonRefDBRepositoryTest()
        {
            _logger = new Mock<ILogger<CommonRefDBRepository>>();
            _commonRefDBServices = new Mock<ICommonRefDBServices>();
            _repo = new CommonRefDBRepository(_commonRefDBServices.Object);

        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommonRefDBRepository(null));
        }
        [Theory]
        [InlineData("WELS44171C")]
        public async Task GetAllCampaignDetails_Success(string campaignCode)
        {
            var campaignData = TestDataProvider.GetTestData<Infrastructure.Models.Campaign>("CampaignManagementMS\\Campaign");
            _commonRefDBServices.Setup(x => x.GetAllCampaignDetails("WELS44171C")).ReturnsAsync(campaignData);
            var result = await _repo.GetCampaignDetails(campaignCode);
            Assert.NotNull(result);
            Assert.Equal("WELS44171C", result.CampaignCode);
            Assert.Equal("07/11/2022", result.CampaignStartDate.Value.ToString("MM/dd/yyyy"));
            Assert.Equal("07/03/2023", result.CampaignEndDate.Value.ToString("MM/dd/yyyy"));
            Assert.Equal("1-2NGEETY", result.CampaignId);
        }

        [Theory]
        [InlineData("1-19GLQ3S", "WELS44171C", "11/07/2022", "03/07/2023", "test_name", "desc", "link", true, "xccv", "test_type", "testuser")]
        public async Task UpdateCampaign_Success(string campaignId, string campaignCode, string startDate, string endDate, string campaignName, string description, string linkToTemplate, bool registrationRequiredFlag, string status, string type, string initiatedBy)
        {
            _commonRefDBServices.Setup(x => x.UpdateCampaignDetails(campaignId, campaignCode, startDate, endDate, campaignName, description, linkToTemplate, registrationRequiredFlag, status, type, initiatedBy)).ReturnsAsync("success");
            var result = await _repo.UpdateCampaign(campaignId, campaignCode, startDate, endDate, campaignName, description, linkToTemplate, registrationRequiredFlag, status, type, initiatedBy);
            Assert.NotNull(result);
            Assert.Equal("success", result);
        }
    }
}
