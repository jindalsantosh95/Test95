using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Tests
{
    public class EnrolmentCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<EnrolmentEventCommandHandler>> _logger;

        public EnrolmentCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<EnrolmentEventCommandHandler>>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EnrolmentEventCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EnrolmentEventCommandHandler(_mediator.Object, null));
        }

    }
}
