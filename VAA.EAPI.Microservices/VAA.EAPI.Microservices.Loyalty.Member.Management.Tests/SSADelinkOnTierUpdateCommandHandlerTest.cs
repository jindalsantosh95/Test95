using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class SSADelinkOnTierUpdateCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly SSADelinkOnTierUpdateCommandHandler _handler;
        private readonly Mock<ILogger<SSADelinkOnTierUpdateCommandHandler>> _logger;

        public SSADelinkOnTierUpdateCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<SSADelinkOnTierUpdateCommandHandler>>();
            _handler = new SSADelinkOnTierUpdateCommandHandler(_mockMediator.Object, _logger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SSADelinkOnTierUpdateCommandHandler(null, null));

            Assert.Throws<ArgumentNullException>(() => new SSADelinkOnTierUpdateCommandHandler(null, _logger.Object));

            Assert.Throws<ArgumentNullException>(() => new SSADelinkOnTierUpdateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void HandlerShouldThrowExceptiononNullSSADelinkOnTierUpdateCommand()
        {
            SSADelinkOnTierUpdateCommand command = null;

            var resp = _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(resp.Exception);
        }

        [Fact]
        public void HandlerShouldReturnErroronInvalidOldTierCode()
        {
            SSADelinkOnTierUpdateCommand command = new SSADelinkOnTierUpdateCommand()
            {
                InitiatedBy = "InitiatedBy",
                CustomerId = "CustomerId001",
                OldTier = "Silver",
                NewTier = "Red"
            };

            var resp = _handler.Handle(command, CancellationToken.None);

            Assert.False(resp.Result.IsSuccess);
        }

        [Fact]
        public void HandlerShouldReturnErroronInvalidNewTierCode()
        {
            SSADelinkOnTierUpdateCommand command = new SSADelinkOnTierUpdateCommand()
            {
                InitiatedBy = "InitiatedBy",
                CustomerId = "CustomerId001",
                OldTier = "Gold",
                NewTier = "Gold"
            };

            var resp = _handler.Handle(command, CancellationToken.None);

            Assert.False(resp.Result.IsSuccess);
        }

        [Fact]
        public void HandlerShouldReturnSuccessOnValidUpdateCommand()
        {
            SSADelinkOnTierUpdateCommand command = new SSADelinkOnTierUpdateCommand()
            {
                InitiatedBy = "InitiatedBy",
                CustomerId = "CustomerId001",
                OldTier = "Gold",
                NewTier = "Silver"
            };

            var delinkOnTierUpdateResponse = new Response<string>
            {
                Status = Status.OK,
                Value = Constants.Application.EventInitiatedMessage
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<SSADelinkOnTierUpdateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(delinkOnTierUpdateResponse);

            var resp = _handler.Handle(command, CancellationToken.None);

            Assert.True(resp.Result.IsSuccess);
        }
    }
}
