using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;
using Xunit;
using Xunit.Sdk;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignUnassignRequestCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignUnassignRequestCommandHandler>> _logger;
        private readonly CampaignUnassignRequestCommandHandler _handler;
        private readonly Mock<IValidator<CampaignUnassignRequestData>> _validator;

        public CampaignUnassignRequestCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignUnassignRequestCommandHandler>>();
            _validator = new Mock<IValidator<CampaignUnassignRequestData>>();
            _handler = new CampaignUnassignRequestCommandHandler(_mediator.Object, _logger.Object, _validator.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.CampaignUnassignResponseUrl, "http://localhost:7071/api/campaignunassign/response/{correlationid}");
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUnassignRequestCommandHandler(_mediator.Object, null, _validator.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUnassignRequestCommandHandler(null, _logger.Object, _validator.Object));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            CampaignUnassignRequestCommand campaignData = TestDataProvider.GetTestData<CampaignUnassignRequestCommand>("CampaignManagementMS\\CampaignUnassignValid");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None));
            _validator
                .Setup(validator => validator.ValidateAsync(campaignData, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            var result = _handler.Handle(campaignData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void Handler_Success_With_Error()
        {
            CampaignUnassignRequestCommand campaignData = TestDataProvider.GetTestData<CampaignUnassignRequestCommand>("CampaignManagementMS\\CampaignUnassignInValid");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Returns(default(Task));
            var validationFailure = new ValidationFailure("ErrorMessage", "something went wrong");
            _validator
               .Setup(validator => validator.ValidateAsync(campaignData, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult(new[] { validationFailure }));
            var result = _handler.Handle(campaignData, System.Threading.CancellationToken.None);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void Handler_Faliure_With_Exception()
        {
            CampaignUnassignRequestCommand campaignData = TestDataProvider.GetTestData<CampaignUnassignRequestCommand>("CampaignManagementMS\\CampaignUnassignInValid");
            _validator
                .Setup(validator => validator.ValidateAsync(campaignData, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(campaignData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}