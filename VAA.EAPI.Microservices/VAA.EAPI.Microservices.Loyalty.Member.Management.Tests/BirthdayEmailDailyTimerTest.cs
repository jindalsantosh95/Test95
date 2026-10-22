using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Timer;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class BirthdayEmailDailyTimerTest
    {
        private readonly Mock<ILogger<BirthdayEmailDailyTimer>> _logger;
        private readonly Mock<IMediator> _mediator;
        private readonly BirthdayEmailDailyTimer _birthdayEmailDailyTimer;
        public BirthdayEmailDailyTimerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<BirthdayEmailDailyTimer>>();
            _birthdayEmailDailyTimer = new BirthdayEmailDailyTimer(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new BirthdayEmailDailyTimer(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new BirthdayEmailDailyTimer(_mediator.Object, null));
        }

        [Fact]
        public void Timer_Success()
        {
            _mediator.Setup(m => m.Send(It.IsAny<BirthdayEmailCommand>(), System.Threading.CancellationToken.None));
            var Output = _birthdayEmailDailyTimer.Run(new TimerInfo
            {
                ScheduleStatus = new ScheduleStatus()
                {
                    Last = DateTime.Now,
                    Next = DateTime.Now,
                }
            });
            Assert.Null(Output.Exception);
        }
    }
}
