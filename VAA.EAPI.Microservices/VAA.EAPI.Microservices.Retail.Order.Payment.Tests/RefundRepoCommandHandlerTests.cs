using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class RefundRepoCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<RefundAPIRepoCommandHanlder>> _logger;
        private readonly RefundAPIRepoCommandHanlder _handler;
        public RefundRepoCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<RefundAPIRepoCommandHanlder>>();
            _handler = new RefundAPIRepoCommandHanlder(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RefundAPIRepoCommandHanlder(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RefundAPIRepoCommandHanlder(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccess()
        {
            var result = await _handler.Handle(new RefundAPIRepoCommand(), CancellationToken.None);

            Assert.True(result.Equals(false));
        }
    }
}
