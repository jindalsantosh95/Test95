using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;
using Xunit;
using Xunit.Sdk;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionUpdateRequestCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<PromotionUpdateRequestCommandHandler>> _mockLogger;
        private readonly PromotionUpdateRequestCommandHandler _handler;
        private readonly Mock<IValidator<PromotionUpdateRequestData>> _validator;

        public PromotionUpdateRequestCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<PromotionUpdateRequestCommandHandler>>();
            _validator = new Mock<IValidator<PromotionUpdateRequestData>>();
            _handler = new PromotionUpdateRequestCommandHandler(_mockMediator.Object, _mockLogger.Object, _validator.Object);
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionUpdateRequestCommandHandler(_mockMediator.Object, null, _validator.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new PromotionUpdateRequestCommandHandler(null, _mockLogger.Object, _validator.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new PromotionUpdateRequestCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            var promotionUpdateRequestCommand = TestDataProvider.GetTestData<PromotionUpdateRequestCommand>("CampaignManagementMS\\PromotionUpdateValid");
            _validator.Setup(validator => validator.ValidateAsync(promotionUpdateRequestCommand, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockMediator.Setup(x => x.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var result = _handler.Handle(promotionUpdateRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithInvalidRequest()
        {
            var promotionUpdateRequestCommand = TestDataProvider.GetTestData<PromotionUpdateRequestCommand>("CampaignManagementMS\\PromotionUpdateInValid");
            var validationFailure = new ValidationFailure("ErrorMessage", "something went wrong");
            _validator.Setup(validator => validator.ValidateAsync(promotionUpdateRequestCommand, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult(new[] { validationFailure }));
            _mockMediator.Setup(x => x.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var result = _handler.Handle(promotionUpdateRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerFaliureWithError()
        {
            var promotionUpdateRequestCommand = TestDataProvider.GetTestData<PromotionUpdateRequestCommand>("CampaignManagementMS\\PromotionUpdateValid");
            _validator.Setup(validator => validator.ValidateAsync(promotionUpdateRequestCommand, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockMediator.Setup(x => x.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var result = _handler.Handle(promotionUpdateRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}
