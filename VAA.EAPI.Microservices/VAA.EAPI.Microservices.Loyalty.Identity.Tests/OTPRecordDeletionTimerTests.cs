using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Timer;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class OTPRecordDeletionTimerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OTPRequestTimerCommandHandler _handler;


        public OTPRecordDeletionTimerTests()
        {
            _mediator = new Mock<IMediator>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OTPRecordDeletionTimer(null));
        }
    }
}
