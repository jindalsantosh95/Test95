using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class TargetedMembershipReportingCommandHandlerTest
    {
        private readonly Mock<IReportingDBServices> _reportingDBServices;
        private readonly Mock<ILogger<TargetedMembershipReportingCommandHandler>> _logger;
        private readonly TargetedMembershipReportingCommandHandler _handler;

        public TargetedMembershipReportingCommandHandlerTest()
        {
            _reportingDBServices = new Mock<IReportingDBServices>();
            _logger = new Mock<ILogger<TargetedMembershipReportingCommandHandler>>();
            _handler = new TargetedMembershipReportingCommandHandler(_reportingDBServices.Object, _logger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipReportingCommandHandler(null, _logger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipReportingCommandHandler(_reportingDBServices.Object, null));
        }

        [Fact]
        public void HandlerShouldSuccessWithNoError()
        {
            var req = TestDataProvider.GetTestData<TargetedMembershipReportingCommand>("CampaignManagementMS\\TargetedMembershipDeleteCommandData");
            _reportingDBServices.Setup(x => x.DeleteTargetedMembership(req.CustomerId, req.DocumentId)).ReturnsAsync("Success");
            var result = _handler.Handle(req, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Equal("Success", result.Result.Value.Value);
        }

        [Fact]
        public void HandlerShouldSuccessWithError()
        {
            var req = TestDataProvider.GetTestData<TargetedMembershipReportingCommand>("CampaignManagementMS\\TargetedMembershipDeleteCommandData");
            _reportingDBServices.Setup(x => x.DeleteTargetedMembership(req.CustomerId, req.DocumentId)).ReturnsAsync("Failure");
            var result = _handler.Handle(req, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Equal("Failure", result.Result.Value.Value);
        }
    }
}
