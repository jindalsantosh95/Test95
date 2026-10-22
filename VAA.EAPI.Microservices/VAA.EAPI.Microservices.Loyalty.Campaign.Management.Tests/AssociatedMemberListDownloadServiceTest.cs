using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class AssociatedMemberListDownloadServiceTest
    {
        private readonly AssociatedMemberListDownloadService _assocMemListDownloadService;

        public AssociatedMemberListDownloadServiceTest()
        {
            _assocMemListDownloadService = new AssociatedMemberListDownloadService();

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CreateXmlShouldSuccess()
        {
            var result = _assocMemListDownloadService.CreateXlStatementAndUploadToBlob("XYZ", new List<AssociatedMemberData>() { new AssociatedMemberData() { MemberNumber = "123" } });

            Assert.True(result.IsCompleted);
        }
    }
}
