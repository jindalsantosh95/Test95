using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.SSRToPNR;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class SSRtoPNREventDataMapperTest
    {
        public SSRtoPNREventDataMapperTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CreateEventGridSchemaDataTest_Success()
        {
            var request = TestDataProvider.GetTestData<AddFCNumberToPNRCommand>("MemberManagement\\MemberDetailsToAddSSRList");
            var addSSREvent = SSRtoPNREventDataMapper.MemberDetailsToFQTVSSREventDataMapper(request.MemberDeatilsToAddSSR.FirstOrDefault());
            Assert.Equal("test", addSSREvent.ChannelId);
        }

        [Fact]
        public void CreateEventGridSchemaDataTest_Failure()
        {
            var request = TestDataProvider.GetTestData<AddFCNumberToPNRCommand>("MemberManagement\\MemberDetailsToAddSSRList");
            var addSSREvent = SSRtoPNREventDataMapper.MemberDetailsToFQTVSSREventDataMapper(request.MemberDeatilsToAddSSR.FirstOrDefault());
            Assert.NotEqual("test1", addSSREvent.ChannelId);
        }
    }
}
