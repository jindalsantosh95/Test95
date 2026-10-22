using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BirthdayBonus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class BirthdayBonusRuleSetEventMapperTest
    {
        public BirthdayBonusRuleSetEventMapperTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EventMapperDataSuccess()
        {
            BirthdayBonusMemberDetails member = TestDataProvider.GetTestData<List<BirthdayBonusMemberDetails>>("MemberManagement\\MemberContactData").FirstOrDefault();
            Product productDetail = TestDataProvider.GetTestData<Product>("MemberManagement\\ProductData");
            string status = "Active";
            BirthdayBonusRuleSetEntity ruleSetEntity = BirthdayBonusRuleSetEventMapper.MapBirthdayBonusRuleSetEventData(member, productDetail, status);
            var cloudEvent = BirthdayBonusRuleSetEventMapper.MapBirthdayBonusRuleSetEvent(ruleSetEntity);
            Assert.Equal("Birthday Bonus", ruleSetEntity.ProductName);
            Assert.NotNull(cloudEvent.Data);
        }
    }
}
