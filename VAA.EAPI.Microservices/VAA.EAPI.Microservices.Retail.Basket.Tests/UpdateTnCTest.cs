using MediatR;
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
    public sealed class UpdateTnCTest : IClassFixture<ConfigurationFixture>
    {
        private static readonly Guid basketId = Guid.Parse("4AC7C22C-3105-4F5C-B0D7-575F0E78F3EF");
        private static readonly IDistributedCacheService cache = new DistributedCacheService();
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<HandlerTests>> _mockLogger;
        private readonly Mock<ILogger<CreateBasketRequestHandler>> _mockcreateLogger;
        private readonly Mock<ILogger<UpdateTnCCommandHandler>> _mocktncLogger;
        private readonly Mock<ILogger<UpdateContactDetailsCommandHandler>> _mockcontactLogger;
        private readonly Mock<ILogger<UpdatePassengerCommandHandler>> _mockpassengerLogger;

        static UpdateTnCTest()
        {
            Domain.Basket basket = new() { BasketId = basketId };

            cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);
        }

        private ConfigurationFixture configurationFixture;

        public UpdateTnCTest(ConfigurationFixture configurationFixture)
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<HandlerTests>>();
            _mockcreateLogger = new Mock<ILogger<CreateBasketRequestHandler>>();
            _mocktncLogger = new Mock<ILogger<UpdateTnCCommandHandler>>();
            _mockpassengerLogger = new Mock<ILogger<UpdatePassengerCommandHandler>>();
            _mockcontactLogger = new Mock<ILogger<UpdateContactDetailsCommandHandler>>();

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
        public async Task UpdateTermsAndConditionsFailureWhenBasketIdMissingInCache()
        {
            Guid basketId = Guid.NewGuid();

            UpdateTnCCommandHandler handler = new(cache, _mocktncLogger.Object);

            var response = await handler.Handle(new UpdateTnCCommand() { Id = basketId }, CancellationToken.None);

            Assert.True(response.IsFailure);
        }

        [Fact]
        public async Task UpdateTnCReturnSuccesWithInValidStatus()
        {
            await PrepareBaseket();

            UpdateTnCCommandHandler handler = new(cache, _mocktncLogger.Object);

            var termNCondition = new TermsAndConditions()
            {
                Accepted = false,
                Validation = "INVALID"
            };
            var response = await handler.Handle(new UpdateTnCCommand() { Id = basketId, TermsAndConditions = termNCondition }, CancellationToken.None);

            var responseValue = response.Value.Value.TermsAndConditions.Validation;
            Assert.Equal("INVALID", responseValue);
        }


        [Fact]
        public async Task UpdateTnCReturnSuccesWithValidStatus()
        {
            await PrepareBaseket();

            UpdateTnCCommandHandler handler = new(cache, _mocktncLogger.Object);

            var termNCondition = new TermsAndConditions()
            {
                Accepted = true,
                Validation = "INVALID"
            };
            var response = await handler.Handle(new UpdateTnCCommand() { Id = basketId, TermsAndConditions = termNCondition }, CancellationToken.None);

            var responseValue = response.Value.Value.TermsAndConditions.Validation;
            Assert.Equal("VALID", responseValue);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task UpdateValidateBasketReturnSuccesWithValidStatus()
        {
            await PrepareBaseket();

            UpdatePassengerCommandHandler handlerPassenger = new(cache, _mockpassengerLogger.Object);

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
            var responsePassenger = await handlerPassenger.Handle(new UpdatePassengerCommand() { Id = basketId, PeopleId = "0", People = person }, CancellationToken.None);

            UpdateContactDetailsCommandHandler handlerContact = new(_mockcontactLogger.Object, cache);

            var contact = new Domain.ContactDetails()
            {
                EmailAddress = "testFirstName.testLastName@domain.com",
                PhoneNumber = "11234567892",
                Validation = "INVALID",
                CountryCode = "41",
                AlternativePhoneNumber = "1234567890",
                AcceptEmail = true,
                AcceptPost = false,
                AcceptTelephone = false,
                AcceptTextMessage = false
            };
            var responseContact = await handlerContact.Handle(new UpdateContactDetailCommand() { Id = basketId, ContactDetails = contact }, CancellationToken.None);


            UpdateTnCCommandHandler handler = new(cache, _mocktncLogger.Object);

            var termNCondition = new TermsAndConditions()
            {
                Accepted = true,
                Validation = "INVALID"
            };
            var responseTnC = await handler.Handle(new UpdateTnCCommand() { Id = basketId, TermsAndConditions = termNCondition }, CancellationToken.None);

            var validationPassenger = responsePassenger.Value.Value.People.FirstOrDefault()?.Validation;
            var validationContact = responseContact.Value.Value.ContactDetails.Validation;
            var validationTnC = responseTnC.Value.Value.TermsAndConditions.Validation;
            var validationBasket = responseTnC.Value.Value.Validation;
            Assert.Equal("VALID", validationPassenger);
            Assert.Equal("VALID", validationContact);
            Assert.Equal("VALID", validationTnC);
            Assert.Equal("VALID", validationBasket);
        }



    }
}