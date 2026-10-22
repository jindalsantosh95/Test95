using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.TargetedMembership;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class TargetedMembershipDocUpdateCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<TargetedMembershipDocUpdateCommandHandler>> _logger;
        private readonly TargetedMembershipDocUpdateCommandHandler _handler;

        public TargetedMembershipDocUpdateCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<TargetedMembershipDocUpdateCommandHandler>>();
            _handler = new TargetedMembershipDocUpdateCommandHandler(_mediator.Object, _logger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDocUpdateCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TargetedMembershipDocUpdateCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDocUpdateCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDocUpdateCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void Handler_Success_With_Exception()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDocUpdateCommand>("CampaignManagementMS\\CampaignRegisterInvalid");
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDocUpdateCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            var request = TestDataProvider.GetTestData<TargetedMembershipDocUpdateCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDocUpdateCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}
