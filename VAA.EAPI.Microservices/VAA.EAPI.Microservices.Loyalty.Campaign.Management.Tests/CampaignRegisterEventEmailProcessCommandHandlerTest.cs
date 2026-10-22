using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignRegisterEventEmailProcessCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<EmailEventRequestProcessCommandHandler>> _logger;
        private readonly EmailEventRequestProcessCommandHandler _handler;

        public CampaignRegisterEventEmailProcessCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<EmailEventRequestProcessCommandHandler>>();
            _handler = new EmailEventRequestProcessCommandHandler(_mediator.Object, _logger.Object);
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EmailEventRequestProcessCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EmailEventRequestProcessCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            var request = TestDataProvider.GetTestData<EmailEventRequestProcessCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<EmailEventRequestProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            var request = TestDataProvider.GetTestData<EmailEventRequestProcessCommand>("CampaignManagementMS\\CampaignRegisterCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<EmailEventRequestProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerSuccessWithNoErrorNonCorporateCampaings()
        {
            var request = TestDataProvider.GetTestData<EmailEventRequestProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            _mediator.Setup(m => m.Send(It.IsAny<EmailEventRequestProcessCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }
    }
}