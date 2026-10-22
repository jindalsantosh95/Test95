using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Email.Entities;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Email.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Infrastructure.Email.Tests
{
    public class CommunicationRepositoryTests
    {
        private readonly Mock<IWorkflowClient> _mockIWorkflowClient;
        private readonly Mock<ILogger<CommunicationRepository>> _logger;
        private readonly CommunicationRepository _repository;
        public CommunicationRepositoryTests()
        {
            _mockIWorkflowClient = new Mock<IWorkflowClient>();
            _logger = new Mock<ILogger<CommunicationRepository>>();
            _repository = new CommunicationRepository(_logger.Object, _mockIWorkflowClient.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommunicationRepository(null, _mockIWorkflowClient.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenWorkFlowCLientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommunicationRepository(_logger.Object, null));
        }

        [Fact]
        public async Task ReturnTrue_WhenNotifySuccessAsync()
        {
            var request = TestDataProvider.GetTestData<SendEmailRequest>("Infrastructure.Email\\SendEmailRequest");

            _mockIWorkflowClient.Setup(m => m.SendEmailCommunication(It.IsAny<WorkFlowRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyValuePair<bool, string>(true, @"{ ""success"": true}"));

            var result = await _repository.Notify(request, "123-ascv-1233", CancellationToken.None);
            Assert.IsType<bool>(result);
            Assert.True(result);

        }

        [Fact]
        public async Task ReturnTrue_WhenNotifyFailureAsync()
        {
            var request = TestDataProvider.GetTestData<SendEmailRequest>("Infrastructure.Email\\SendEmailRequest");

            _mockIWorkflowClient.Setup(m => m.SendEmailCommunication(It.IsAny<WorkFlowRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyValuePair<bool, string>(true, @"{ ""success"": false}"));

            var result = await _repository.Notify(request, "123-ascv-1233", CancellationToken.None);
            Assert.IsType<bool>(result);
            Assert.False(result);

        }
    }
}
