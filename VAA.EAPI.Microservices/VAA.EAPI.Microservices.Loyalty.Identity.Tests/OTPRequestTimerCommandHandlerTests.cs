using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class OTPRequestTimerCommandHandlerTests
    {
        private readonly Mock<ILogger<OTPRequestTimerCommandHandler>> _mockLogger;
        private readonly Mock<IMediator> _mediator;
        private readonly OTPRequestTimerCommandHandler _handler;
        private readonly Mock<TableStorageService<GenerateOtpDbEntities>> _tableStorageService;

        public OTPRequestTimerCommandHandlerTests()
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();
            _mediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<OTPRequestTimerCommandHandler>>();
            _tableStorageService = new Mock<TableStorageService<GenerateOtpDbEntities>>();
            _handler = new OTPRequestTimerCommandHandler(_mediator.Object, _mockLogger.Object, _tableStorageService.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OTPRequestTimerCommandHandler(_mediator.Object, null, _tableStorageService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OTPRequestTimerCommandHandler(null, _mockLogger.Object, _tableStorageService.Object));
        }
    }
}
