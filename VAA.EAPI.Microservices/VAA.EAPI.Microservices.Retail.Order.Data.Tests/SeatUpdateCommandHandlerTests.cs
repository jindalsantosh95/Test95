using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCSeatUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCSeatUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class SeatUpdateCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly Mock<IMediator> _mediator;
        private readonly SeatUpdateCommandHandler _handler;
        private readonly OrderDocumentRepoQueryHandler _orderDocumentRepoQueryHandler;
        private readonly CosmosRepoCommandHandler _cosmosRepoCommandHandler;
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly HttpConnector _httpConnector;
        private readonly IMediator _mediatorConnector;
        private readonly IServiceProvider _serviceProvider;

        public SeatUpdateCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _mediator = new Mock<IMediator>();
            _serviceProvider = new DefaultServiceProvider();
            _mediatorConnector = new Mediator(_serviceProvider);
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _orderDocumentRepoQueryHandler = new OrderDocumentRepoQueryHandler(_mediatorConnector);
            _cosmosRepoCommandHandler = new CosmosRepoCommandHandler(_mediatorConnector);
            _handler = new SeatUpdateCommandHandler(_mediator.Object, _orderDocumentRepoQueryHandler , _cosmosRepoCommandHandler);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SeatUpdateCommandHandler(null, _orderDocumentRepoQueryHandler , _cosmosRepoCommandHandler));
        }

        [Fact]
        public async Task Handler_ShouldNotThrowExceptionWithGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SeatFulfilmentResponse>("SeatMS\\SeatFulfilmentResponse-Cash");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync([TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc")]);

            //Act
            var result = await _handler.Handle(new SeatUpdateCommand() { Data = Json.Serialize(request)}, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldThrowExceptionWhenExceptionOccurs()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SeatFulfilmentResponse>("SeatMS\\SeatFulfilmentResponse-Cash");
            _mediator.Setup(x => x.Send(It.IsAny<OrderDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            //Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new SeatUpdateCommand() { Data = Json.Serialize(request) }, new CancellationToken()));
        }
    }
}
