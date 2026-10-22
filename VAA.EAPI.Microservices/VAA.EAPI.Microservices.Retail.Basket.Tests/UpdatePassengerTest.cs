using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Basket.Application.Command;
using VAA.EAPI.Microservices.Retail.Basket.Domain;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Basket.Tests
{
    public sealed class UpdatePassengerTest : IClassFixture<ConfigurationFixture>
    {
        private static readonly Guid basketId = Guid.Parse("4AC7C22C-3105-4F5C-B0D7-575F0E78F3EF");
        private static readonly IDistributedCacheService cache = new DistributedCacheService();
        private readonly Mock<ILogger<CreateBasketRequestHandler>> _mockcreateLogger;
        private readonly Mock<ILogger<UpdatePassengerCommandHandler>> _mockpassengerLogger;

        static UpdatePassengerTest()
        {
            Domain.Basket basket = new() { BasketId = basketId };

            cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);
        }

        private ConfigurationFixture configurationFixture;

        public UpdatePassengerTest(ConfigurationFixture configurationFixture)
        {
            _mockpassengerLogger = new Mock<ILogger<UpdatePassengerCommandHandler>>();
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

        private async Task PrepareBaseketDuplicate()
        {
            CreateBasketRequestHandler createHandler = new(cache, _mockcreateLogger.Object);

            Domain.Basket basket = new()
            {
                BasketId = basketId
            };
            var personOne = new Person() { Id = "0", Type = "ADULT" };
            var personTwo = new Person() { Id = "1", Type = "ADULT" };
            basket.People = new[] { personOne, personTwo };
            var responseBasket = await createHandler.Handle(new CreateBasketRequest() { Basket = basket }, CancellationToken.None);
        }

        [Fact]
        public async Task UpdatePassengerFailureWhenBasketIdMissingInCache()
        {
            Guid basketId = Guid.NewGuid();

            UpdatePassengerCommandHandler handler = new(cache, _mockpassengerLogger.Object);

            var response = await handler.Handle(new UpdatePassengerCommand() { Id = basketId }, CancellationToken.None);

            Assert.True(response.IsFailure);
        }

        [Fact]
        public async Task UpdatePassengerReturnSucces()
        {
            await PrepareBaseket();

            UpdatePassengerCommandHandler handler = new(cache, _mockpassengerLogger.Object);

            var person = new Domain.Person()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                DateOfBirth = Convert.ToDateTime("10-JAN-2000"),
                Gender = "Male",
                Type = "ADULT",
                Id = "0"
            };
            var response = await handler.Handle(new UpdatePassengerCommand() { Id = basketId, PeopleId = "0", People = person }, CancellationToken.None);

            Assert.True(response.IsSuccess);

        }

        [Fact]
        public async Task UpdatePassengerValidateFirstNameReturnSuccesWithInvalidStatus()
        {
            await PrepareBaseket();

            UpdatePassengerCommandHandler handler = new(cache, _mockpassengerLogger.Object);

            var person = new Domain.Person()
            {
                FirstName = "TestFirstName length more than expected",
                LastName = "TestLastName",
                DateOfBirth = Convert.ToDateTime("10-JAN-2000"),
                Validation = "INVALID",
                Gender = "Male",
                Type = "ADULT",
                Id = "0"
            };
            var response = await handler.Handle(new UpdatePassengerCommand() { Id = basketId, PeopleId = "0", People = person }, CancellationToken.None);

            var responseValue = response.Value.Value.People.FirstOrDefault().Validation;
            Assert.Equal("INVALID", responseValue);

        }

        [Fact]
        public async Task UpdatePassengerValidateLastNameReturnSuccesWithInvalidStatus()
        {
            await PrepareBaseket();

            UpdatePassengerCommandHandler handler = new(cache, _mockpassengerLogger.Object);

            var person = new Domain.Person()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName length more than expected",
                DateOfBirth = Convert.ToDateTime("10-JAN-2000"),
                Gender = "Male",
                Type = "ADULT",
                Id = "0"
            };
            var response = await handler.Handle(new UpdatePassengerCommand() { Id = basketId, PeopleId = "0", People = person }, CancellationToken.None);

            var responseValue = response.Value.Value.Validation;
            Assert.Equal("INVALID", responseValue);

        }

        [Fact]
        public async Task UpdatePassengerValidateDuplicatePassengerReturnSuccesWithInvalidStatus()
        {
            await PrepareBaseketDuplicate();

            UpdatePassengerCommandHandler handler = new(cache, _mockpassengerLogger.Object);

            var personOne = new Domain.Person()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                DateOfBirth = Convert.ToDateTime("10-JAN-2000"),
                Gender = "Male",
                Type = "ADULT",
                Title = "",
                Id = "0",
                Validation = "VALID"
            };

            var personTwo = new Domain.Person()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                DateOfBirth = Convert.ToDateTime("10-JAN-2000"),
                Gender = "Male",
                Type = "ADULT",
                Title = "",
                Id = "1",
                Validation = "VALID"
            };
            var response = await handler.Handle(new UpdatePassengerCommand() { Id = basketId, PeopleId = "0", People = personOne }, CancellationToken.None);


            var responseDuplicate = await handler.Handle(new UpdatePassengerCommand() { Id = basketId, PeopleId = "0", People = personTwo }, CancellationToken.None);

            var responseValue = response.Value.Value.People.FirstOrDefault().Validation;
            Assert.Equal("INVALID", responseValue);

        }

        [Fact]
        public async Task UpdatePassengerReturnSuccesWithValidStatus()
        {
            await PrepareBaseket();

            UpdatePassengerCommandHandler handler = new(cache, _mockpassengerLogger.Object);

            var person = new Domain.Person()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                DateOfBirth = Convert.ToDateTime("10-JAN-2000"),
                Gender = "Male",
                Type = "ADULT",
                Id = "0",
                Validation = "VALID"

            };
            var response = await handler.Handle(new UpdatePassengerCommand() { Id = basketId, PeopleId = "0", People = person }, CancellationToken.None);

            var responseValue = response.Value.Value.People.FirstOrDefault().Validation;
            Assert.Equal("VALID", responseValue);
            Assert.True(response.IsSuccess);

        }
    }
}