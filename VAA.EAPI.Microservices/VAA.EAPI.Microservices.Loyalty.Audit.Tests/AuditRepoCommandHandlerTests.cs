using MediatR;
using Moq;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests
{
    public class AuditRepoCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly AuditRepoCommandHandler _handler;

        public AuditRepoCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _handler = new AuditRepoCommandHandler(_mockMediator.Object);

            AuditTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditRepoCommandHandler(null));
        }

        [Fact]
        public async Task HandlerShouldSucceed()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<CosmosModifyRepoCommand>(), CancellationToken.None)).ReturnsAsync(new CSharpFunctionalExtensions.Result<Unit>());

            // Act
            var result = await _handler.Handle(new AuditRepoCommand(new CosmosDocumentEvent()), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
