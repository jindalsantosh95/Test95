using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class VRedRepoHandlerTests
    {

        private readonly Mock<ILogger<VRedRepoHandler>> _mockLogger;

        public VRedRepoHandlerTests()
        {
            VRedCommon.CreateTestEnvironmentSettings();

            _mockLogger = new Mock<ILogger<VRedRepoHandler>>();
        }

        /// <summary>
        /// Test when the request parameter to the handler is NULL
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_NullRequest_ShouldThrowArgumentException()
        {
            // Arrange
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            VRedRepoHandler handler = new VRedRepoHandler(mockMediator.Object, _mockLogger.Object);

            // Act
            async Task act() => await handler.Handle(null, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        /// <summary>
        /// Test when the request parameter to the handler is not null, but the primary property of the request is.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_NullCreateCustomerEntities_ShouldThrowArgumentException()
        {
            // Arrange
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            VRedRepoHandler handler = new VRedRepoHandler(mockMediator.Object, _mockLogger.Object);
            VredRepoQuery request = new VredRepoQuery(null);

            // Act
            async Task act() => await handler.Handle(request, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact]
        public async Task Handle_ServiceRequestFailed_ShouldThrowVRedException()
        {

            // Arrange
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<IRequest<VRedEnrolmentResponse>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VRedEnrolmentResponse()
            {
                ErrorResponse = new VRedErrorResponse()
                {
                    Code = "1",
                    Message = "Something failed"
                }
            });

            VRedRepoHandler handler = new VRedRepoHandler(mockMediator.Object, _mockLogger.Object);
            VredRepoQuery request = new VredRepoQuery(new Microservices.Loyalty.Vred.Domain.CreateCustomerEntities()
            {
            });

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_ServiceRequestSucceeded_ShouldThrowVRedException()
        {

            // Arrange
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<IRequest<VRedEnrolmentResponse>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VRedEnrolmentResponse());

            VRedRepoHandler handler = new VRedRepoHandler(mockMediator.Object, _mockLogger.Object);
            VredRepoQuery request = new VredRepoQuery(new Microservices.Loyalty.Vred.Domain.CreateCustomerEntities());

            // Act
            var testResult = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<bool>(testResult);
            Assert.True(testResult);
        }
    }
}
