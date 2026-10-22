using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionDelinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionDelinking;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignPromotionDelinkRequestCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignPromotionDelinkRequestCommandHandler>> _logger;
        private readonly CampaignPromotionDelinkRequestCommandHandler _handler;
        private readonly Mock<IValidator<CampaignPromotionDelinkRequestData>> _validator;

        public CampaignPromotionDelinkRequestCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignPromotionDelinkRequestCommandHandler>>();
            _validator = new Mock<IValidator<CampaignPromotionDelinkRequestData>>();
            _handler = new CampaignPromotionDelinkRequestCommandHandler(_mediator.Object, _logger.Object, _validator.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ServiceBusTopicName, "dummyValue1");
            Environment.SetEnvironmentVariable(Constants.Environment.CampaignPromotionLinkResponseUrl, "http://localhost:7071/api/campaignpromotionlink/response/{correlationid}");
            CampaignManagementHelpers.CreateTestEnvironmentSettings();

        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionDelinkRequestCommandHandler(_mediator.Object, null, _validator.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionDelinkRequestCommandHandler(null, _logger.Object, _validator.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenValidatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionDelinkRequestCommandHandler(_mediator.Object, _logger.Object, null));
        }

        [Fact]
        public void Handler_Success_With_Error()
        {
            CampaignPromotionDelinkRequestCommand delinkingData = TestDataProvider.GetTestData<CampaignPromotionDelinkRequestCommand>("CampaignManagementMS\\CampaignPromotionLinkingInvalid");
            _validator.Setup(v => v.Validate(delinkingData).IsValid).Returns(false);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None));
            var result = _handler.Handle(delinkingData, System.Threading.CancellationToken.None);
            Assert.NotNull(result.Result.Value.Errors);
        }
        [Fact]
        public void Handler_Faliure_With_Exception()
        {
            CampaignPromotionDelinkRequestCommand delinkingData = TestDataProvider.GetTestData<CampaignPromotionDelinkRequestCommand>("CampaignManagementMS\\CampaignPromotionLinkingInvalid");
            _validator.Setup(v => v.Validate(delinkingData).IsValid).Returns(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(delinkingData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            CampaignPromotionDelinkRequestCommand delinkingData = TestDataProvider.GetTestData<CampaignPromotionDelinkRequestCommand>("CampaignManagementMS\\CampaignPromotionLinkingInvalid");
            _validator.Setup(v => v.Validate(delinkingData).IsValid).Returns(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None));
            var result = _handler.Handle(delinkingData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }
    }
}
