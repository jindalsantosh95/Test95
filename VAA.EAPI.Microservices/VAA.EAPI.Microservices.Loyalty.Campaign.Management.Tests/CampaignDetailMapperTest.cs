using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignDetailMapperTest
    {
        [Fact]
        public void MapCamppaignData_Success()
        {
            var request = TestDataProvider.GetTestData<Infrastructure.Models.Campaign>("CampaignManagementMS\\Campaign");
            Assert.Equal("WELS44171C", request.CampaignCode);
            Assert.Contains("07/11/2022", request.StartDate.Value.ToString("MM/dd/yyyy"));
            Assert.Contains("07/03/2023", request.EndDate.Value.ToString("MM/dd/yyyy"));
            Assert.Equal("1-2NGEETY", request.RowId);
        }
    }
}
