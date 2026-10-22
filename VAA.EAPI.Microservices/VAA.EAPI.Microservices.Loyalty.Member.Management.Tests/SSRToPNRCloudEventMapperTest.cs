using System;
using System.Linq;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.SSRToPNR;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class SSRToPNRCloudEventMapperTest
    {
        public SSRToPNRCloudEventMapperTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CreateEventGridSchemaDataTest_Success()
        {
            var request = TestDataProvider.GetTestData<AddFCNumberToPNRCommand>("MemberManagement\\MemberDetailsToAddSSRList");
            var addSSREvent = SSRtoPNREventDataMapper.MemberDetailsToFQTVSSREventDataMapper(request.MemberDeatilsToAddSSR.FirstOrDefault());
            var cloudEvent = SSRToPNRCloudEventMapper.CommandToEventMapper(addSSREvent, Guid.NewGuid().ToString());
            Assert.Equal(EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.AddSSREvent.ToString())), cloudEvent.Type);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }

        [Fact]
        public void CreateEventGridSchemaDataTest_Failure()
        {
            var request = TestDataProvider.GetTestData<AddFCNumberToPNRCommand>("MemberManagement\\MemberDetailsToAddSSRList");
            var addSSREvent = SSRtoPNREventDataMapper.MemberDetailsToFQTVSSREventDataMapper(request.MemberDeatilsToAddSSR.FirstOrDefault());
            var cloudEvent = SSRToPNRCloudEventMapper.CommandToEventMapper(addSSREvent, Guid.NewGuid().ToString());
            Assert.NotEqual("2.0", cloudEvent.Specversion);
        }
    }
}
