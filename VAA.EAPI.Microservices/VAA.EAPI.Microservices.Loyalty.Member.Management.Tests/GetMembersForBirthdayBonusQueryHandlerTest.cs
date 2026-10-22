using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class GetMembersForBirthdayBonusQueryHandlerTest
    {
        private readonly Mock<IReportingDBServices> _reportingDBService;
        private readonly Mock<ILogger<GetMembersForBirthdayBonusQueryHandler>> _logger;
        private readonly GetMembersForBirthdayBonusQueryHandler getMembersForBirthdayBonusQueryHandler;
        public GetMembersForBirthdayBonusQueryHandlerTest()
        {
            _reportingDBService = new Mock<IReportingDBServices>();
            _logger = new Mock<ILogger<GetMembersForBirthdayBonusQueryHandler>>();
            getMembersForBirthdayBonusQueryHandler = new GetMembersForBirthdayBonusQueryHandler(_reportingDBService.Object, _logger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetMembersForBirthdayBonusQueryHandler(_reportingDBService.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenDbServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetMembersForBirthdayBonusQueryHandler(null, _logger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var request = new GetMembersForBirthdayBonusQuery();
            var memberBirthdayBonusResponse = TestDataProvider.GetTestData<List<BirthdayBonusMemberDetails>>("MemberManagement\\MemberContactData");
            _reportingDBService.Setup(m => m.GetMembersDetailsForBirthdayBonus(It.IsAny<DateTime>())).ReturnsAsync(memberBirthdayBonusResponse);
            var result = await getMembersForBirthdayBonusQueryHandler.Handle(request, CancellationToken.None);
            Assert.True(result.Count() == memberBirthdayBonusResponse.Count());
        }

        [Fact]
        public async Task HandlerSuccessWithException()
        {
            var request = new GetMembersForBirthdayBonusQuery();
            var memberBirthdayBonusResponse = TestDataProvider.GetTestData<List<BirthdayBonusMemberDetails>>("MemberManagement\\MemberContactData");
            _reportingDBService.Setup(m => m.GetMembersDetailsForBirthdayBonus(It.IsAny<DateTime>())).ThrowsAsync(new Exception());
            await Assert.ThrowsAsync<Exception>(() => getMembersForBirthdayBonusQueryHandler.Handle(request, CancellationToken.None));
        }
    }
}
