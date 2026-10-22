using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.Communication;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.CRM.Email.Tests
{
    public class CommunicationEventCommandHandlerTest
    {
        private readonly Mock<ILogger<CommunicationEventCommandHandler>> _mockLogger;
        private CommunicationEventCommandHandler _handler;
        private readonly Mock<ICommunicationRepository> _mockCommunicationRepository;

        public CommunicationEventCommandHandlerTest()
        {
            _mockLogger = new Mock<ILogger<CommunicationEventCommandHandler>>();
            _mockCommunicationRepository = new Mock<ICommunicationRepository>();
            _handler = new CommunicationEventCommandHandler(_mockLogger.Object, _mockCommunicationRepository.Object);
            EmailTestsCommon.CreateTestEnvironmentSettings();

        }
        [Fact]
        public void ThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommunicationEventCommandHandler(null, _mockCommunicationRepository.Object));

        }

        [Fact]
        public void ThrowNullException_WhenRepoObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommunicationEventCommandHandler(_mockLogger.Object, null));

        }

        [Fact]
        public async Task ThrowNullException_WhenRequestIsNull()
        {
            await Assert.ThrowsAsync<NullReferenceException>(() =>
            _handler.Handle(null, CancellationToken.None));

        }

        [Fact]
        public async Task ReturnTrue_WhenNotifySuccess()
        {
            var request = TestDataProvider.GetTestData<CommunicationEventCommand>("EmailMS\\CommunicationCommand");

            _mockCommunicationRepository.Setup(m => m.Notify(It.IsAny<SendEmailRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            _handler = new CommunicationEventCommandHandler(_mockLogger.Object, _mockCommunicationRepository.Object);

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.IsType<bool>(result);
            Assert.True(result);

        }

        [Fact]
        public async Task ReturFalse_WhenNotifyFail()
        {
            var request = TestDataProvider.GetTestData<CommunicationEventCommand>("EmailMS\\CommunicationCommand");

            _mockCommunicationRepository.Setup(m => m.Notify(It.IsAny<SendEmailRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.IsType<bool>(result);
            Assert.False(result);

        }
    }
}
