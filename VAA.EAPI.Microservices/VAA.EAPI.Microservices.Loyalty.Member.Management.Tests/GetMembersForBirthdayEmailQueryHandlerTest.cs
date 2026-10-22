using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class GetMembersForBirthdayEmailQueryHandlerTest
    {
        private readonly Mock<IReportingDBServices> _reportingDBService;
        private readonly Mock<ILogger<GetMembersForBirthdayEmailQueryHandler>> _logger;
        private readonly GetMembersForBirthdayEmailQueryHandler getMembersForBirthdayEmailQueryHandler;
        public GetMembersForBirthdayEmailQueryHandlerTest()
        {
            _reportingDBService = new Mock<IReportingDBServices>();
            _logger = new Mock<ILogger<GetMembersForBirthdayEmailQueryHandler>>();
            getMembersForBirthdayEmailQueryHandler = new GetMembersForBirthdayEmailQueryHandler(_reportingDBService.Object, _logger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetMembersForBirthdayEmailQueryHandler(_reportingDBService.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenDbServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetMembersForBirthdayEmailQueryHandler(null, _logger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var request = new GetMembersForBirthdayEmailQuery();
            var memberBirthdayEmailResponse = TestDataProvider.GetTestData<List<MemberContactData>>("MemberManagement\\MemberContactData");
            _reportingDBService.Setup(m => m.GetMembersBirthdayEmailDetails()).ReturnsAsync(memberBirthdayEmailResponse);
            var result = await getMembersForBirthdayEmailQueryHandler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task HandlerSuccessWithException()
        {
            var request = new GetMembersForBirthdayEmailQuery();
            var result = await getMembersForBirthdayEmailQueryHandler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
        }
    }
}
