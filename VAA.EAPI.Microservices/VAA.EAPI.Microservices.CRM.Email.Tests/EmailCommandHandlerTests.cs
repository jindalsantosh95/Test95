using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Microservices.CRM.Email.Application.Command;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Email.Tests
{
    public class EmailCommandHandlerTests
    {
        private readonly Mock<ILogger<EmailCommandHandler>> _mockLogger;
        private readonly EmailCommandHandler _handler;
        private readonly Mock<ICcpRepository> repo;

        public EmailCommandHandlerTests()
        {
            _mockLogger = new Mock<ILogger<EmailCommandHandler>>();
            repo = new Mock<ICcpRepository>();
            _handler = new EmailCommandHandler(_mockLogger.Object, repo.Object);

        }
        [Fact]
        public void ThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EmailCommandHandler(null, repo.Object));

        }

        [Fact]
        public void ThrowNullException_WhenRepoObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EmailCommandHandler(_mockLogger.Object, null));

        }

        [Fact]
        public async Task ReturnFalse_WhenRequestIsEmpty()
        {
            var command = new EmailEventProcessCommand();
            var result = await _handler.Handle(command, CancellationToken.None);
            Assert.IsType<bool>(result);
            Assert.False(result);

        }
    }
}
