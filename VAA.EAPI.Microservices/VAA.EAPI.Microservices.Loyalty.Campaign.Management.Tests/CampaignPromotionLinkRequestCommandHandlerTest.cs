using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionLinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionLinking;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignPromotionLinkRequestCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignPromotionLinkRequestCommandHandler>> _logger;
        private readonly CampaignPromotionLinkRequestCommandHandler _handler;
        private readonly Mock<IValidator<CampaignPromotionLinkRequestData>> _validator;

        public CampaignPromotionLinkRequestCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignPromotionLinkRequestCommandHandler>>();
            _validator = new Mock<IValidator<CampaignPromotionLinkRequestData>>();
            _handler = new CampaignPromotionLinkRequestCommandHandler(_mediator.Object, _logger.Object, _validator.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ServiceBusTopicName, "dummyValue1");
            Environment.SetEnvironmentVariable(Constants.Environment.CampaignPromotionLinkResponseUrl, "http://localhost:7071/api/campaignpromotionlink/response/{correlationid}");
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionLinkRequestCommandHandler(_mediator.Object, null, _validator.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionLinkRequestCommandHandler(null, _logger.Object, _validator.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenValidatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionLinkRequestCommandHandler(_mediator.Object, _logger.Object, null));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            CampaignPromotionLinkRequestCommand linkingData = TestDataProvider.GetTestData<CampaignPromotionLinkRequestCommand>("CampaignManagementMS\\CampaignPromotionLinkingValid");
            _validator.Setup(v => v.Validate(linkingData).IsValid).Returns(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None));
            var result = _handler.Handle(linkingData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void Handler_Success_With_Error()
        {
            CampaignPromotionLinkRequestCommand linkingData = TestDataProvider.GetTestData<CampaignPromotionLinkRequestCommand>("CampaignManagementMS\\CampaignPromotionLinkingInvalid");
            _validator.Setup(v => v.Validate(linkingData).IsValid).Returns(false);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Returns(default(Task));
            var result = _handler.Handle(linkingData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }
        [Fact]
        public void Handler_Faliure_With_Exception()
        {
            CampaignPromotionLinkRequestCommand linkingData = TestDataProvider.GetTestData<CampaignPromotionLinkRequestCommand>("CampaignManagementMS\\CampaignPromotionLinkingInvalid");
            _validator.Setup(v => v.Validate(linkingData).IsValid).Returns(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(linkingData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}
