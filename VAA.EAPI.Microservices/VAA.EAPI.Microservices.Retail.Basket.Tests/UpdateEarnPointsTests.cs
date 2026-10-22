using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using OpenQA.Selenium;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Basket.Application.Command;
using VAA.EAPI.Microservices.Retail.Basket.Domain;
using VAA.EAPI.Microservices.Retail.Basket.Domain.FCEarnPoints;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Basket.Tests
{
    public sealed class UpdateEarnPointsTests : IClassFixture<ConfigurationFixture>
    {
        private static readonly Guid basketId = Guid.Parse("4AC7C22C-3105-4F5C-B0D7-575F0E78F3EF");
        private static readonly IDistributedCacheService cache = new DistributedCacheService();
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<HandlerTests>> _mockLogger;
        private readonly Mock<ILogger<CreateBasketRequestHandler>> _mockcreateLogger;

        static UpdateEarnPointsTests()
        {
            Domain.Basket basket = new() { BasketId = basketId };

            cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);
        }

        private ConfigurationFixture configurationFixture;

        public UpdateEarnPointsTests(ConfigurationFixture configurationFixture)
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<HandlerTests>>();
            _mockcreateLogger = new Mock<ILogger<CreateBasketRequestHandler>>();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            this.configurationFixture = configurationFixture;
        }

        private async Task PrepareBaseket()
        {
            CreateBasketRequestHandler createHandler = new(cache, _mockcreateLogger.Object);

            Domain.Basket basket = new()
            {
                BasketId = basketId
            };
            var person = new Person() { Id = "0", Type = "ADULT" };
            basket.People = new[] { person };
            var responseBasket = await createHandler.Handle(new CreateBasketRequest() { Basket = basket }, CancellationToken.None);
        }

        [Fact]
        public async Task UpdateEarnPointsFailureWhenBasketIdMissingInCache()
        {
            Guid basketId = Guid.NewGuid();

            UpdateEarnPointsCommandHandler handler = new(cache);

            var response = await handler.Handle(new UpdateEarnPointsCommand() { Id = basketId }, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(EAPI.Domain.Entities.Common.Status.NotFound, response.Value.Status);
        }

        [Fact]
        public async Task UpdateEarnPointsReturnSucces()
        {
            await PrepareBaseket();

            UpdateEarnPointsCommandHandler handler = new(cache);

            var points = new EarnPoints()
            {
                EarnTierPoints = 25,
                EarnVirginPoints = 1000
            };

            var response = await handler.Handle(new UpdateEarnPointsCommand() { Id = basketId, EarnPoints = points }, CancellationToken.None); ;

            var responseValue = response.Value.Value.EarnPoints;
            Assert.True(response.IsSuccess);
            Assert.True(responseValue.EarnTierPoints == 25);
            Assert.True(responseValue.EarnVirginPoints == 1000);
        }
    }
}