using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class VRedEnrolmentHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<VredEnrolmentHandler>> _mockLogger;
        private readonly VredEnrolmentHandler _handler;

        public VRedEnrolmentHandlerTests()
        {
            VRedCommon.CreateTestEnvironmentSettings();

            _mockMediator = new Mock<IMediator>();
            // Assumes the SEND method always returns TRUE (The actual response is not captured)
            _mockMediator.Setup(m => m.Send(It.IsAny<VredRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);

            _mockLogger = new Mock<ILogger<VredEnrolmentHandler>>();
            _handler = new VredEnrolmentHandler(_mockMediator.Object, _mockLogger.Object);

        }

        /// <summary>
        /// Tests VRedEnrolmentHandler.Handle for a NULL CreateCustomerEntities 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_NullRequest_ShouldThrowArgumentException()
        {
            // Arrange

            // Act
            async Task act() => await _handler.Handle(null, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }


        /// <summary>
        /// Tests VRedEnrolmentHandler.Handle for an empty CreateCustomerEntities 
        /// 
        /// Empty meaning object instantiated, but no properties set
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_EmptyRequest_ShouldReturnTrue()
        {
            // Arrange
            var cust = new CreateCustomerEntities(); // Create a completely empty entity

            // Act
            var result = await _handler.Handle(cust, CancellationToken.None);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True(result);
        }


        /// <summary>
        /// Tests VRedEnrolmentHandler.Handle for a CreateCustomerEntities 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_RequestWithData_ShouldReturnTrue()
        {

            // Arrange
            var cust = new CreateCustomerEntities()
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = "1984-06-22"
            };

            // Act
            var result = await _handler.Handle(cust, CancellationToken.None);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True(result);
        }
    }
}
