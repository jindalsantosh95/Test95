using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BirthdayEmail;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class BirthdayEmailTriggeringEventMapperTest
    {
        public BirthdayEmailTriggeringEventMapperTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CreateEventGridSchemaDataTest_Success()
        {
            var request = TestDataProvider.GetTestData<List<MemberContactData>>("MemberManagement\\MemberContactData").FirstOrDefault();
            var emailEvent = BirthdayEmailTriggeringEventMapper.CreateEventGridSchemaData(request);
            Assert.Equal("test.email@fly.virgin.com", emailEvent.Data.To);
        }
        [Fact]
        public void CreateEventGridSchemaDataTest_Failure()
        {
            var request = TestDataProvider.GetTestData<List<MemberContactData>>("MemberManagement\\MemberContactData").FirstOrDefault();
            var emailEvent = BirthdayEmailTriggeringEventMapper.CreateEventGridSchemaData(request);
            Assert.NotEqual("test1.email@fly.virgin.com", emailEvent.Data.To);
        }
    }
}
