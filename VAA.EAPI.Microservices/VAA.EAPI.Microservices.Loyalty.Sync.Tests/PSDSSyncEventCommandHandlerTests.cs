using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class PSDSSyncEventCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<PSDSSyncEventCommandHandler>> _logger;
        private readonly PSDSSyncEventCommandHandler _handler;

        public PSDSSyncEventCommandHandlerTests()
        {
            _logger = new Mock<ILogger<PSDSSyncEventCommandHandler>>();
            _mediator = new Mock<IMediator>();
            _handler = new PSDSSyncEventCommandHandler(_logger.Object, _mediator.Object);
             LoyaltySyncTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PSDSSyncEventCommandHandler(null, _mediator.Object));
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PSDSSyncEventCommandHandler(_logger.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnTrue_ForGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<PSDSSyncEventCommand>("LoyaltySyncMS\\PSDSSyncEventCommandRequest");

            _mediator.Setup(x => x.Send(It.IsAny<PSDSSyncRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), CancellationToken.None)).ReturnsAsync(true);

            //Act
            var result = await _handler.Handle(request, new CancellationToken());

            //Assert
            Assert.True(result.Value);
        }

        [Fact]
        public async Task Handler_ThrowError_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnFalse_WhenPSDSFeatureFlagisFalse()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PSDSSyncEventCommand>("LoyaltySyncMS\\PSDSSyncEventCommandRequest");
            request.Data.EnrollmentChannel = "test";
            System.Environment.SetEnvironmentVariable(Constants.Environment.EnablePSDSSync, "false");
           
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), CancellationToken.None)).ReturnsAsync(true);

            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());

            //Assert
            Assert.False(result.Value);
        }
    }
}
