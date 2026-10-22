using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Basket.Application.Command;
using VAA.EAPI.Microservices.Retail.Basket.Application.Query;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Basket.Tests
{
    public sealed class HandlerTests : IClassFixture<ConfigurationFixture>
    {
        private static readonly Guid basketId = Guid.Parse("4AC7C22C-3105-4F5C-B0D7-575F0E78F3EF");
        private static readonly IDistributedCacheService cache = new DistributedCacheService();
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<HandlerTests>> _mockLogger;
        private readonly Mock<ILogger<CreateBasketRequestHandler>> _mockcreateLogger;
        private readonly Mock<ILogger<DeleteBasketRequestHandler>> _mockdeleteLogger;
        private readonly Mock<ILogger<UpdateContactDetailsCommandHandler>> _mockcontactLogger;
        private readonly Mock<ILogger<UpdatePassengerCommandHandler>> _mockpassengerLogger;
        private readonly Mock<ILogger<UpdateTnCCommandHandler>> _mocktermsLogger;
        private readonly Mock<ILogger<LoadBasketRequestHandler>> _mockloadLogger;

        static HandlerTests()
        {
            Domain.Basket basket = new() { BasketId = basketId };

            cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);
        }

        private ConfigurationFixture configurationFixture;

        public HandlerTests(ConfigurationFixture configurationFixture)
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<HandlerTests>>();
            _mockcreateLogger = new Mock<ILogger<CreateBasketRequestHandler>>();
            _mockdeleteLogger = new Mock<ILogger<DeleteBasketRequestHandler>>();
            _mocktermsLogger = new Mock<ILogger<UpdateTnCCommandHandler>>();
            _mockpassengerLogger = new Mock<ILogger<UpdatePassengerCommandHandler>>();
            _mockcontactLogger = new Mock<ILogger<UpdateContactDetailsCommandHandler>>();
            _mockloadLogger = new Mock<ILogger<LoadBasketRequestHandler>>();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            this.configurationFixture = configurationFixture;
        }

        [Fact]
        public async Task CreateBasket()
        {
            Guid basketId = Guid.NewGuid();

            CreateBasketRequestHandler handler = new(cache, _mockcreateLogger.Object);

            var response = await handler.Handle(new CreateBasketRequest() { Basket = new() { BasketId = basketId } }, CancellationToken.None);

            var basket = response.Value?.Value;

            Assert.True(basket?.BasketId.Equals(basketId), "Expecting guid for new basket to match.");
        }

        [Fact]
        public async Task LoadBasket()
        {
            LoadBasketRequestHandler handler = new(cache, _mockloadLogger.Object);

            var response = await handler.Handle(new LoadBasketRequest() { Id = basketId }, CancellationToken.None);

            var basket = response.Value?.Value;

            Assert.True(basket?.BasketId.Equals(basketId), $"Expecting basket '{basketId}'.");
        }

        [Fact]
        public async Task DeleteBasket()
        {
            Guid basketId = Guid.NewGuid();
            CreateBasketRequestHandler createBasketRequestHandler = new(cache, _mockcreateLogger.Object);

            var createBasketResponse = await createBasketRequestHandler.Handle(new CreateBasketRequest() { Basket = new() { BasketId = basketId } }, CancellationToken.None);

            var basket = createBasketResponse.Value?.Value;

            DeleteBasketRequestHandler deleteBasketRequestHandler = new(cache, _mockdeleteLogger.Object);

            var deleteBasketResponse = await deleteBasketRequestHandler.Handle(new DeleteBasketRequest() { Id = basket.BasketId }, CancellationToken.None);

            Assert.True(deleteBasketResponse.Value.Value, "Basket should have been deleted.");

            LoadBasketRequestHandler loadBasketRequestHandler = new(cache, _mockloadLogger.Object);

            var loadBasketResponse = await loadBasketRequestHandler.Handle(new LoadBasketRequest() { Id = basket.BasketId }, CancellationToken.None);

            if (loadBasketResponse.IsFailure)
                Assert.Throws<CSharpFunctionalExtensions.ResultFailureException>(() => loadBasketResponse.Value);
            else
            {
                basket = loadBasketResponse.Value?.Value;

                Assert.True(basket == null, "Basket should have been deleted.");
            }
        }
    }
}