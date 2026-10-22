using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Infrastructure.Tests
{
    public class CosmosReaderRepoCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CosmosReaderRepoQueryHandler>> _logger;
        private readonly CosmosReaderRepoQueryHandler _handler;
        public CosmosReaderRepoCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CosmosReaderRepoQueryHandler>>();
            _handler = new CosmosReaderRepoQueryHandler(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CosmosReaderRepoQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CosmosReaderRepoQueryHandler(_mediator.Object, null));
        }

        /// <summary>
        /// When an valid member number is passed, then a List<object> will be returned by the handler,
        /// which is then serialised and returned to the consumer.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_ShouldReturnListOfObject()
        {
            // Arrange
            CosmosReaderRepoQuery command = new();
            _mediator.Setup(m => m.Send(It.IsAny<CosmosReaderRepoQuery>(), CancellationToken.None)).ReturnsAsync(() =>
            {
                return new List<object>();
            });

            // Act
            var testResult = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Empty(testResult);
        }

        /// <summary>
        /// When an invalid member number provided, then a <null> will be returned by the handler,
        /// which is then serialised as an empty string 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_ShouldReturnEmptyStringForInvalidMemberQuery()
        {
            // Arrange
            CosmosReaderRepoQuery command = new();
            _mediator.Setup(m => m.Send(It.IsAny<CosmosReaderRepoQuery>(), CancellationToken.None)).ReturnsAsync(() =>
            {
                return null;
            });

            // Act
            var testResult = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Empty(testResult);
        }
    }
}
