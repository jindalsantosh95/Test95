using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Basket.Application.Command;
using VAA.EAPI.Microservices.Retail.Basket.Domain;
using VAA.EAPI.Microservices.Retail.Basket.Domain.PointsCurrencyConverter;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Basket.Tests
{
    public sealed class UpdatePaymentTest : IClassFixture<ConfigurationFixture>
    {
        private static readonly Guid basketId = Guid.Parse("4AC7C22C-3105-4F5C-B0D7-575F0E78F3EF");
        private static readonly IDistributedCacheService cache = new DistributedCacheService();
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<HandlerTests>> _mockLogger;
        private readonly Mock<ILogger<CreateBasketRequestHandler>> _mockcreateLogger;
        private readonly Mock<ILogger<UpdatePaymentCommandHandler>> _mocksPaymentLogger;


        static UpdatePaymentTest()
        {
            Domain.Basket basket = new() { BasketId = basketId };

            cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);
        }

        private ConfigurationFixture configurationFixture;

        public UpdatePaymentTest(ConfigurationFixture configurationFixture)
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<HandlerTests>>();
            _mockcreateLogger = new Mock<ILogger<CreateBasketRequestHandler>>();
            _mocksPaymentLogger = new Mock<ILogger<UpdatePaymentCommandHandler>>();

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
        public async Task UpdatePaymentReturnFailureWhenBasketIdMissingInCache()
        {
            Guid basketId = Guid.NewGuid();

            UpdatePaymentCommandHandler handler = new(cache, _mocksPaymentLogger.Object);

            _ = await Assert.ThrowsAsync<NullReferenceException>(async () => await handler.Handle(new UpdatePaymentCommand() { Id = basketId }, CancellationToken.None));
        }

        [Fact]
        public async Task UpdatePaymentWithPayWithPointsReturnSucces()
        {
            await PrepareBaseket();

            UpdatePaymentCommandHandler handler = new(cache, _mocksPaymentLogger.Object);

            var pointsEquivalentCurrency = new LoyaltyPointsEquivalentCurrency()
            {
                Amount = new() { BaseCurrency = "GBP", MonetaryAmount = "16.50" },
                LoyaltyPoints = "3000" 
            };

            var paymentSession = new PaymentSession()
            {
                AmountToPay = (decimal)1901.00F,
                SelectedPayWithPointsOption = pointsEquivalentCurrency
            };
            var response = await handler.Handle(new UpdatePaymentCommand() { Id = basketId, PaymentSessions = new[] { paymentSession } }, CancellationToken.None);

            var responseValue = response.Value.Value.PaymentSessions;
            Assert.True(response.IsSuccess);
            Assert.Equal(responseValue.First().SelectedPayWithPointsOption?.LoyaltyPoints, pointsEquivalentCurrency.LoyaltyPoints);
            Assert.Equal(responseValue.First().SelectedPayWithPointsOption?.Amount, pointsEquivalentCurrency.Amount);

        }
    }
}