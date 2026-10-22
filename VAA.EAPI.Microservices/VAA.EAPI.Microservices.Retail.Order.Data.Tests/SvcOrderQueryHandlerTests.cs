using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class SvcOrderQueryHandlerTests
    {
        public SvcOrderQueryHandlerTests()
        {
            OrderDataTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task HandleAsync_ReturnsAllTrue_WhenPnrAndNamesMatch()
        {
            var repository = new StubSvcOrderRepository(CreateOrder("ORDER-123", "Jane", "Smith"));
            var handler = new SvcOrderQueryHandler(repository);

            var result = await handler.HandleAsync(" abc123 ", " jane ", " smith ", CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.True(result.Value.Pnr);
            Assert.False(result.Value.IsGroup);
            Assert.True(result.Value.FirstName);
            Assert.True(result.Value.Surname);
            Assert.Equal("ABC123", repository.Pnr);
        }

        [Fact]
        public async Task HandleAsync_ReturnsGroupAndSkipsNameValidation_WhenBookingTypeIsGroup()
        {
            var order = CreateOrder("ORDER-123", "Jane", "Smith");
            order.CustomerDetails.First().Bookingtype = " group ";
            var repository = new StubSvcOrderRepository(order);
            var handler = new SvcOrderQueryHandler(repository);

            var result = await handler.HandleAsync("ABC123", "Jane", "Smith", CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.True(result.Value.Pnr);
            Assert.True(result.Value.IsGroup);
            Assert.False(result.Value.FirstName);
            Assert.False(result.Value.Surname);
        }

        [Fact]
        public async Task HandleAsync_ReturnsAllTrue_WhenStoredNamesContainWhitespace()
        {
            var repository = new StubSvcOrderRepository(CreateOrder("ORDER-123", " Jane ", " Smith "));
            var handler = new SvcOrderQueryHandler(repository);

            var result = await handler.HandleAsync("ABC123", "Jane", "Smith", CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.True(result.Value.Pnr);
            Assert.True(result.Value.FirstName);
            Assert.True(result.Value.Surname);
        }

        [Fact]
        public async Task HandleAsync_ReturnsPnrAndFirstNameTrue_WhenSurnameDoesNotMatch()
        {
            var repository = new StubSvcOrderRepository(CreateOrder("ORDER-123", "Jane", "Jones"));
            var handler = new SvcOrderQueryHandler(repository);

            var result = await handler.HandleAsync("ABC123", " jane ", " smith ", CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.True(result.Value.Pnr);
            Assert.True(result.Value.FirstName);
            Assert.False(result.Value.Surname);
        }

        [Fact]
        public async Task HandleAsync_RequiresFirstAndLastNameToMatchSamePassenger()
        {
            var repository = new StubSvcOrderRepository(
                new OrderDoc
                {
                    OrderID = "ORDER-123",
                    CustomerDetails =
                    [
                        new CustomerDetail { FirstName = "Jane", LastName = "Jones" },
                        new CustomerDetail { FirstName = "John", LastName = "Smith" }
                    ]
                });
            var handler = new SvcOrderQueryHandler(repository);

            var result = await handler.HandleAsync("ABC123", "jane", "smith", CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.True(result.Value.Pnr);
            Assert.True(result.Value.FirstName);
            Assert.False(result.Value.Surname);
        }

        [Fact]
        public async Task HandleAsync_ThrowsArgumentException_WhenPnrIsMissing()
        {
            var repository = new StubSvcOrderRepository(CreateOrder("ORDER-123", "Jane", "Smith"));
            var handler = new SvcOrderQueryHandler(repository);

            await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(" ", "Jane", "Smith", CancellationToken.None));

            Assert.Equal(0, repository.CallCount);
        }

        [Fact]
        public async Task HandleAsync_ReturnsPnrTrueAndNamesFalse_WhenNamesAreOmitted()
        {
            var repository = new StubSvcOrderRepository(CreateOrder("ORDER-123", "Jane", "Smith"));
            var handler = new SvcOrderQueryHandler(repository);

            var result = await handler.HandleAsync("ABC123", null, null, CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.True(result.Value.Pnr);
            Assert.False(result.Value.FirstName);
            Assert.False(result.Value.Surname);
        }

        [Fact]
        public async Task HandleAsync_ReturnsPnrTrueAndNamesFalse_WhenFirstNameDoesNotMatch()
        {
            var repository = new StubSvcOrderRepository(CreateOrder("ORDER-123", "Jane", "Smith"));
            var handler = new SvcOrderQueryHandler(repository);

            var result = await handler.HandleAsync("ABC123", "John", "Jones", CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.True(result.Value.Pnr);
            Assert.False(result.Value.FirstName);
            Assert.False(result.Value.Surname);
            Assert.Equal(1, repository.CallCount);
        }

        [Fact]
        public async Task HandleAsync_ReturnsAllFalse_WhenPnrDoesNotMatchAnyOrder()
        {
            var repository = new StubSvcOrderRepository();
            var handler = new SvcOrderQueryHandler(repository);

            var result = await handler.HandleAsync("ABC123", "Jane", "Smith", CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.False(result.Value.Pnr);
            Assert.False(result.Value.FirstName);
            Assert.False(result.Value.Surname);
        }

        private static OrderDoc CreateOrder(string orderId, string firstName, string lastName)
        {
            return new OrderDoc
            {
                OrderID = orderId,
                CustomerDetails =
                [
                    new CustomerDetail { FirstName = firstName, LastName = lastName }
                ]
            };
        }

        private sealed class StubSvcOrderRepository(params OrderDoc[] orders) : ISvcOrderRepository
        {
            public string Pnr { get; private set; } = string.Empty;
            public int CallCount { get; private set; }

            public Task<IReadOnlyCollection<OrderDoc>> GetByPnrAsync(string pnr, CancellationToken cancellationToken)
            {
                Pnr = pnr;
                CallCount++;
                return Task.FromResult<IReadOnlyCollection<OrderDoc>>(orders);
            }
        }
    }
}
