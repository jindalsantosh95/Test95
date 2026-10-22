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
    public class BirthdayBonusDailyTimerTest
    {
        private readonly Mock<ILogger<BirthdayBonusDailyTimer>> _logger;
        private readonly Mock<IMediator> _mediator;
        private readonly BirthdayBonusDailyTimer _birthdayBonusDailyTimer;
        public BirthdayBonusDailyTimerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<BirthdayBonusDailyTimer>>();
            _birthdayBonusDailyTimer = new BirthdayBonusDailyTimer(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new BirthdayBonusDailyTimer(null, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new BirthdayBonusDailyTimer(_mediator.Object, null));
        }

        [Fact]
        public void BirthdayBonusTimerSuccess()
        {
            _mediator.Setup(m => m.Send(It.IsAny<BirthdayBonusCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync("Success");
            var Output = _birthdayBonusDailyTimer.Run(new TimerInfo
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
