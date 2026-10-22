using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionListDownloadServiceTest
    {
        private readonly Mock<ILogger<PromotionListDownloadService>> _mockLogger;
        private readonly PromotionListDownloadService _promotionListDownloadService;

        public PromotionListDownloadServiceTest()
        {
            _mockLogger = new Mock<ILogger<PromotionListDownloadService>>();
            _promotionListDownloadService = new PromotionListDownloadService(_mockLogger.Object);

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PromotionListDownloadService(null));
        }

        [Fact]
        public void CreateXmlShouldSuccess()
        {
            var promoDetails = new List<PromotionsDownload>() { new PromotionsDownload() { PromotionId = "123", PromotionName = "test" } };
            CampaignSearchResponse campaignSearchResponse = TestDataProvider.GetTestData<CampaignSearchResponse>("CampaignManagementMS\\CampaignSearchResponse");

            var result = _promotionListDownloadService.CreateXlStatementAndUploadToBlob(promoDetails);

            Assert.True(result.IsCompleted);
        }
    }
}
