using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BirthdayEmail;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class BirthdayEmailTriggeringEventGridMapperTest
    {
        public BirthdayEmailTriggeringEventGridMapperTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EventDataMapperDataTest_Success()
        {
            var request = TestDataProvider.GetTestData<List<MemberContactData>>("MemberManagement\\MemberContactData").FirstOrDefault();
            var emailEvent = BirthdayEmailTriggeringEventMapper.CreateEventGridSchemaData(request);
            var cloudEvent = BirthdayEmailTriggeringEventGridMapper.EventDataMapper(emailEvent);
            Assert.Equal(EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BirthdayEmailEvent.ToString())), cloudEvent.Type);
            Assert.Equal(EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BirthdayEmailEvent.ToString())), cloudEvent.Subject);
            Assert.Equal(Constants.Application.EventSpecversion, cloudEvent.Specversion);
        }
        [Fact]
        public void EventDataMapperDataTest_Failure()
        {
            var request = TestDataProvider.GetTestData<List<MemberContactData>>("MemberManagement\\MemberContactData").FirstOrDefault();
            var emailEvent = BirthdayEmailTriggeringEventMapper.CreateEventGridSchemaData(request);
            var cloudEvent = BirthdayEmailTriggeringEventGridMapper.EventDataMapper(emailEvent);
            Assert.NotEqual("2.0", cloudEvent.Specversion);
        }
    }
}
