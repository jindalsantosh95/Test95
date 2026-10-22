using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUpdate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignUpdateRequestCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignUpdateRequestCommandHandler>> _logger;
        private readonly CampaignUpdateRequestCommandHandler _handler;

        public CampaignUpdateRequestCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignUpdateRequestCommandHandler>>();
            _handler = new CampaignUpdateRequestCommandHandler(_mediator.Object, _logger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ServiceBusTopicName, "dummyValue1");
            Environment.SetEnvironmentVariable(Constants.Environment.CampaignUpdateResponseUrl, "http://localhost:7071/api/campaignupdate/response/{correlationid}");
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUpdateRequestCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUpdateRequestCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            CampaignUpdateRequestCommand campaignData = TestDataProvider.GetTestData<CampaignUpdateRequestCommand>("CampaignManagementMS\\CampaignUpdateValid");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None));
            var result = _handler.Handle(campaignData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void Handler_Success_With_Error()
        {
            CampaignUpdateRequestCommand campaignData = TestDataProvider.GetTestData<CampaignUpdateRequestCommand>("CampaignManagementMS\\CampaignUpdateInvalid");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Returns(default(Task));
            var result = _handler.Handle(campaignData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void Handler_Faliure_With_Exception()
        {
            CampaignUpdateRequestCommand campaignData = TestDataProvider.GetTestData<CampaignUpdateRequestCommand>("CampaignManagementMS\\CampaignUpdateValid");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(campaignData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}
