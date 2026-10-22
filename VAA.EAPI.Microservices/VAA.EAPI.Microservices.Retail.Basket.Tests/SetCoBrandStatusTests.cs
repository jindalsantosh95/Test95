using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Basket.Application.Command;
using VAA.EAPI.Microservices.Retail.Basket.Application.Query;
using VAA.EAPI.Microservices.Retail.Basket.Domain;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Enums;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Basket.Tests
{
    public class SetCoBrandStatusTests : IClassFixture<ConfigurationFixture>
    {
        private static readonly Guid basketId = Guid.Parse("4AC7C22C-3105-4F5C-B0D7-575F0E78F3EF");
        private static readonly IDistributedCacheService cache = new DistributedCacheService();
        private readonly Mock<ILogger<SetCoBrandStatusCommandHandler>> _mockLogger;
        private readonly Mock<ILogger<LoadBasketRequestHandler>> _mockloadLogger;

        static SetCoBrandStatusTests()
        {
            Domain.Basket basket = new()
            {
                BasketId = basketId
            };
            var person = new Person() { Id = "0", Type = "ADULT" };
            basket.People = new[] { person };
            basket.ContactDetails = new();
            basket.TermsAndConditions = new();

            cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);
        }

        private ConfigurationFixture configurationFixture;

        public SetCoBrandStatusTests(ConfigurationFixture configurationFixture)
        {
            _mockLogger = new Mock<ILogger<SetCoBrandStatusCommandHandler>>();
            _mockloadLogger = new Mock<ILogger<LoadBasketRequestHandler>>();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            this.configurationFixture = configurationFixture;
        }

        [Fact]
        public async Task SetCoBrandStatusHandlerDefaultStatusCheck()
        {
            LoadBasketRequestHandler handler = new(cache, _mockloadLogger.Object);

            var response = await handler.Handle(new LoadBasketRequest() { Id = basketId }, CancellationToken.None);

            var basket = response.Value?.Value;

            Assert.True(basket?.CoBrandStatus.Equals(CoBrandStatusEnum.NotApplied), $"Expecting value '{CoBrandStatusEnum.NotApplied}'.");

        }

        [Fact]
        public async Task SetCoBrandStatusHandlerReturnsFailureWhenBasketNotInCache()
        {
            Guid basketId = Guid.NewGuid();

            SetCoBrandStatusCommandHandler handler = new(_mockLogger.Object, cache);

            var responseBasket = await handler.Handle(new SetCoBrandStatusCommand() { Id = basketId, CoBrandProgressStatus = Enum.Parse<CoBrandStatusEnum>("Initiated") }, CancellationToken.None);

            Assert.True(responseBasket.IsFailure == true, "Basket Not Found");
             
        }


        [Fact]
        public async Task SetCoBrandStatusHandlerSetInitiatedStatus()
        {
            Guid basketId = Guid.NewGuid();

            Domain.Basket basket = new()
            {
                BasketId = basketId
            };
            var person = new Person() { Id = "0", Type = "ADULT" };
            basket.People = new[] { person };
            basket.ContactDetails = new();
            basket.TermsAndConditions = new();

            await cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);

            SetCoBrandStatusCommandHandler handler = new(_mockLogger.Object, cache);

            var responseBasket = await handler.Handle(new SetCoBrandStatusCommand() { Id = basketId, CoBrandProgressStatus = Enum.Parse<CoBrandStatusEnum>("Initiated") }, CancellationToken.None);

            Assert.True(responseBasket.IsSuccess);
            Assert.Equal(CoBrandStatusEnum.Initiated, responseBasket.Value.Value.CoBrandStatus);
        }

        [Fact]
        public async Task SetCoBrandStatusHandlerSetApprovedStatus()
        {
            Guid basketId = Guid.NewGuid();

            Domain.Basket basket = new()
            {
                BasketId = basketId
            };
            var person = new Person() { Id = "0", Type = "ADULT" };
            basket.People = new[] { person };
            basket.ContactDetails = new();
            basket.TermsAndConditions = new();

            await cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);

            SetCoBrandStatusCommandHandler handler = new(_mockLogger.Object, cache);

            var responseBasket = await handler.Handle(new SetCoBrandStatusCommand() { Id = basketId, CoBrandProgressStatus = Enum.Parse<CoBrandStatusEnum>("Approved") }, CancellationToken.None);

            Assert.True(responseBasket.IsSuccess);
            Assert.Equal(CoBrandStatusEnum.Approved, responseBasket.Value.Value.CoBrandStatus);
        }

        [Fact]
        public async Task SetCoBrandStatusHandlerSetRejectedStatus()
        {
            Guid basketId = Guid.NewGuid();

            Domain.Basket basket = new()
            {
                BasketId = basketId
            };
            var person = new Person() { Id = "0", Type = "ADULT" };
            basket.People = new[] { person };
            basket.ContactDetails = new();
            basket.TermsAndConditions = new();

            await cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);

            SetCoBrandStatusCommandHandler handler = new(_mockLogger.Object, cache);

            var responseBasket = await handler.Handle(new SetCoBrandStatusCommand() { Id = basketId, CoBrandProgressStatus = Enum.Parse<CoBrandStatusEnum>("Rejected") }, CancellationToken.None);

            Assert.True(responseBasket.IsSuccess);
            Assert.Equal(CoBrandStatusEnum.Rejected, responseBasket.Value.Value.CoBrandStatus);
        }

        [Fact]
        public async Task SetCoBrandStatusHandlerSetPendingStatus()
        {
            Guid basketId = Guid.NewGuid();

            Domain.Basket basket = new()
            {
                BasketId = basketId
            };
            var person = new Person() { Id = "0", Type = "ADULT" };
            basket.People = new[] { person };
            basket.ContactDetails = new();
            basket.TermsAndConditions = new();

            await cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);


            SetCoBrandStatusCommandHandler handler = new(_mockLogger.Object, cache);

            var responseBasket = await handler.Handle(new SetCoBrandStatusCommand() { Id = basketId, CoBrandProgressStatus = Enum.Parse<CoBrandStatusEnum>("Pending") }, CancellationToken.None);

            Assert.True(responseBasket.IsSuccess);
            Assert.Equal(CoBrandStatusEnum.Pending, responseBasket.Value.Value.CoBrandStatus);
        }

        [Fact]
        public async Task SetCoBrandStatusHandlerSetApprovedInsufficientCreditStatus()
        {
            Guid basketId = Guid.NewGuid();

            Domain.Basket basket = new()
            {
                BasketId = basketId
            };
            var person = new Person() { Id = "0", Type = "ADULT" };
            basket.People = new[] { person };
            basket.ContactDetails = new();
            basket.TermsAndConditions = new();

            await cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);

            SetCoBrandStatusCommandHandler handler = new(_mockLogger.Object, cache);

            var responseBasket = await handler.Handle(new SetCoBrandStatusCommand() { Id = basketId, CoBrandProgressStatus = Enum.Parse<CoBrandStatusEnum>("ApprovedInsufficientCredit") }, CancellationToken.None);

            Assert.True(responseBasket.IsSuccess);
            Assert.Equal(CoBrandStatusEnum.ApprovedInsufficientCredit, responseBasket.Value.Value.CoBrandStatus);
        }

    }
}
