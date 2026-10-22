using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using Xunit;
using Xunit.Sdk;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class SupplementarySilverDelinkingCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly SupplementarySilverDelinkingCommandHandler _handler;
        private readonly Mock<IValidator<SupplementarySilverDelinkingCommand>> _validator;

        public SupplementarySilverDelinkingCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _validator = new Mock<IValidator<SupplementarySilverDelinkingCommand>>();
            _handler = new SupplementarySilverDelinkingCommandHandler(_mockMediator.Object, _validator.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SupplementarySilverDelinkingCommandHandler(null, _validator.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new SupplementarySilverDelinkingCommandHandler(_mockMediator.Object, null));
        }


        [Fact]
        public void HandlerShouldFailWithValidationFailure()
        {
            var request = TestDataProvider.GetTestData<SupplementarySilverDelinkingCommand>("MemberManagement\\SupplementarySilverDeLinkingRequest");
            _validator
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("ErrorMessage", "something went wrong") }));
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.Null(result.Exception);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerShouldFailWithSameCustomerId()
        {
            var request = TestDataProvider.GetTestData<SupplementarySilverDelinkingCommand>("MemberManagement\\SupplementarySilverDeLinkingRequest");
            request.ChildAccount.CustomerId = request.HeadAccount.CustomerId;
            _validator
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.Null(result.Exception);
            Assert.True(result.Result.IsFailure);
        }
    }
}
