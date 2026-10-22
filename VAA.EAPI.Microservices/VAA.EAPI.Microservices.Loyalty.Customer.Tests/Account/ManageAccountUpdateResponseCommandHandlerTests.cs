using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using CSharpFunctionalExtensions;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.Account
{
    public class ManageAccountUpdateResponseCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockmediator;
        private readonly Mock<ILogger<ManageAccountUpdateResponseCommandHandler>> _mockLogger;
        private readonly ManageAccountUpdateResponseCommandHandler _mockHandler;

        public ManageAccountUpdateResponseCommandHandlerTests()
        {
            _mockmediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<ManageAccountUpdateResponseCommandHandler>>();
            _mockHandler = new ManageAccountUpdateResponseCommandHandler(_mockmediator.Object, _mockLogger.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageAccountUpdateResponseCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageAccountUpdateResponseCommandHandler(_mockmediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnSucceess_ForGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<ManageAccountUpdateResponseCommand>("CustomerMS\\Account\\ManageAccountUpdateResponseCommandRequest");

            var updateResponse  = TestDataProvider.GetTestData<ServiceResponse>("CustomerMS\\Account\\ManageAccountUpdateResponse");

            _mockmediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(updateResponse);

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.Equal("Updated Member Address Successfully", result.Value.Value.Value);
            Assert.IsType<Result<Response<ManageAccountUpdateResponse>>>(result);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForFailedUpdate()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<ManageAccountUpdateResponseCommand>("CustomerMS\\Account\\ManageAccountUpdateResponseCommandRequest");

            var updateResponse = TestDataProvider.GetTestData<ServiceResponse>("CustomerMS\\Account\\ManageAccountUpdateResponse");
            updateResponse.Status = ServiceResponseProcessStatus.Error;

            _mockmediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(updateResponse);

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Value.Errors);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Equal("G009", result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForFailingToConnectToStorageTable()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<ManageAccountUpdateResponseCommand>("CustomerMS\\Account\\ManageAccountUpdateResponseCommandRequest");

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Value.Errors);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Equal("G009", result.Value.Errors.First().Code);
        }
    }
}
