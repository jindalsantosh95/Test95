using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class TargetedMemberDocumentServiceTest
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;
        private readonly Mock<ILogger<TargetedMemberDocumentService>> _mockLogger;
        private readonly TargetedMemberDocumentService _mockService;

        public TargetedMemberDocumentServiceTest()
        {
            _mockApiConnector = new Mock<IHttpConnector>();
            _mockLogger = new Mock<ILogger<TargetedMemberDocumentService>>();
            _mockService = new TargetedMemberDocumentService(_mockApiConnector.Object, _mockLogger.Object);
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMemberDocumentService(null, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new TargetedMemberDocumentService(_mockApiConnector.Object, null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task HandlerShouldThrowException()
        {
            var response = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");
            _mockApiConnector.Setup(m => m.GetAsync<TargetedMembershipResponse>(It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
                                                                    It.IsAny<string>())).ReturnsAsync(response);
            await Assert.ThrowsAsync<Exception>(() => _mockService.GetMemberCampaigns("1qeqwr"));
        }
    }
}
