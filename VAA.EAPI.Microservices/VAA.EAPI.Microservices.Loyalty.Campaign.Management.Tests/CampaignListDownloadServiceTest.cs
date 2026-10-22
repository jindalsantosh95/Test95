using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignListDownloadServiceTest
    {
        private readonly Mock<ILogger<CampaignListDownloadService>> _mockLogger;
        private readonly CampaignListDownloadService _campaignListDownloadService;

        public CampaignListDownloadServiceTest()
        {
            _mockLogger = new Mock<ILogger<CampaignListDownloadService>>();
            _campaignListDownloadService = new CampaignListDownloadService(_mockLogger.Object);

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignListDownloadService(null));
        }

        [Fact]
        public void CreateXmlShouldSuccess()
        {
            CampaignSearchResponse campaignSearchResponse = TestDataProvider.GetTestData<CampaignSearchResponse>("CampaignManagementMS\\CampaignSearchResponse");

            var result = _campaignListDownloadService.CreateXlStatementAndUploadToBlob(campaignSearchResponse);

            Assert.True(result.IsCompleted);
        }
    }
}
