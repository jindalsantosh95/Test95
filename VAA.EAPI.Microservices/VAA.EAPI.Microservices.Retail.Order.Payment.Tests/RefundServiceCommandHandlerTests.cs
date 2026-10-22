using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class RefundServiceCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<RefundAPISvcCommandHandler>> _logger;
        private readonly RefundAPISvcCommandHandler _handler;
        public RefundServiceCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<RefundAPISvcCommandHandler>>();
            _handler = new RefundAPISvcCommandHandler(_logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RefundAPISvcCommandHandler(null));
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccess()
        {
            var result = await _handler.Handle(new RefundAPISvcCommand(), CancellationToken.None);

            Assert.True(result.Key);
        }
    }
}
