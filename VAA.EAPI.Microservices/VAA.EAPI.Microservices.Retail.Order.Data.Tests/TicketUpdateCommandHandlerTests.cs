using MediatR;
using Moq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCTicketUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCTicketUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class TicketUpdateCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly Mock<IMediator> _mediator;
        private readonly IMediator _mediators;
        private readonly IServiceProvider _serviceProvider;
        private readonly TicketUpdateCommandHandler _handler;
        private readonly OrderDocumentRepoQueryHandler _orderDocumentRepoQueryHandler;
        private readonly CosmosRepoCommandHandler _cosmosRepoCommandHandler;

        public TicketUpdateCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _mediator = new Mock<IMediator>();
            _serviceProvider = new DefaultServiceProvider();
            _mediators = new Mediator(_serviceProvider);
            _orderDocumentRepoQueryHandler = new OrderDocumentRepoQueryHandler(_mediators);
            _cosmosRepoCommandHandler = new CosmosRepoCommandHandler(_mediators);
            _handler = new TicketUpdateCommandHandler(_mediator.Object, _orderDocumentRepoQueryHandler, _cosmosRepoCommandHandler);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TicketUpdateCommandHandler(null, _orderDocumentRepoQueryHandler, _cosmosRepoCommandHandler));
        }


        [Fact]
        public async Task Handler_ShouldThrowExceptionWhenExceptionOccurs()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<IssueTicketResponse>("SabreOrderDocument\\IssueTicketResponse");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            //Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new TicketUpdateCommand() { Data = Json.Serialize(request) }, new CancellationToken()));
        }
    }
}
