using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.CampaignUnassign;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class TargetedMembershipDeleteEventCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<TargetedMembershipDeleteEventCommandHandler>> _logger;
        private readonly TargetedMembershipDeleteEventCommandHandler _targetedMembershipDeleteHandler;

        public TargetedMembershipDeleteEventCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<TargetedMembershipDeleteEventCommandHandler>>();
            _targetedMembershipDeleteHandler = new TargetedMembershipDeleteEventCommandHandler(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDeleteEventCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDeleteEventCommandHandler(null, _logger.Object));
        }

        [Fact]
        public async Task HandlerShouldReturnSuccess()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDeleteEventCommand>("CampaignManagementMS\\TargetedMembershipDeleteEventData");
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipReportingCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<string> { Value = "Success" });
            var result = await _targetedMembershipDeleteHandler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal("Record is successfully deleted from Reporting DB", result.Value.Value);
        }

        [Fact]
        public async Task HandlerShouldReturnFailure()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDeleteEventCommand>("CampaignManagementMS\\TargetedMembershipDeleteEventData");
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipReportingCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<string> { Value = "Failure" });
            var result = await _targetedMembershipDeleteHandler.Handle(request, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
    }
}
