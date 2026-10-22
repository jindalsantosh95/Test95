using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class LTGEmailTriggerCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;
        private readonly Mock<ILogger<LTGEmailTriggerCommandHandler>> _mockLogger;
        private readonly LTGEmailTriggerCommandHandler _handler;

        public LTGEmailTriggerCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<LTGEmailTriggerCommandHandler>>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _handler = new LTGEmailTriggerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerProvider.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LTGEmailTriggerCommandHandler(null, _mockLogger.Object, _mockCustomerProvider.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new LTGEmailTriggerCommandHandler(_mockMediator.Object, null, _mockCustomerProvider.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new LTGEmailTriggerCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public void HandlerShouldSuccessWithException()
        {
            LTGEmailTriggerCommand command = null;
            var result = _handler.Handle(command, CancellationToken.None);
            Assert.NotNull(result.Exception);
        }

        [Fact]
        public void HandlerShouldSuccessWithValidCammand()
        {
            LTGEmailTriggerCommand command = new LTGEmailTriggerCommand
            {
                InitiatedBy = "InitiatedBy",
                CustomerId = "CustomerId",
                MemberId = "MemberId",
                FirstName = "FirstName",
                LastName = "LastName",
                Email = "Email"
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountEmailTriggerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = _handler.Handle(command, CancellationToken.None);
            Assert.Null(result.Exception);
            Assert.True(result.Result.IsSuccess);
        }

        [Fact]
        public void HandlerShouldSuccessWithInValidCammand()
        {
            LTGEmailTriggerCommand command = new LTGEmailTriggerCommand
            {
                CustomerId = "CustomerId",
                MemberId = "MemberId",
                FirstName = "FirstName",
                LastName = "LastName",
                Email = "Email"
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountEmailTriggerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = _handler.Handle(command, CancellationToken.None);
            Assert.False(result.Result.IsSuccess);
        }
    }
}
