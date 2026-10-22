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
    public sealed class UpdateContactTest : IClassFixture<ConfigurationFixture>
    {
        private static readonly Guid basketId = Guid.Parse("4AC7C22C-3105-4F5C-B0D7-575F0E78F3EF");
        private static readonly IDistributedCacheService cache = new DistributedCacheService();
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<HandlerTests>> _mockLogger;
        private readonly Mock<ILogger<CreateBasketRequestHandler>> _mockcreateLogger;
        private readonly Mock<ILogger<UpdateContactDetailsCommandHandler>> _mockcontactLogger;
        private readonly Mock<ILogger<UpdatePassengerCommandHandler>> _mockpassengerLogger;

        static UpdateContactTest()
        {
            Domain.Basket basket = new() { BasketId = basketId };

            cache.Set(basket.BasketId.ToString("N"), basket, true, Constants.Environment.RedisExpiryTime, TimeSpan.Zero);
        }

        private ConfigurationFixture configurationFixture;

        public UpdateContactTest(ConfigurationFixture configurationFixture)
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<HandlerTests>>();
            _mockcreateLogger = new Mock<ILogger<CreateBasketRequestHandler>>();
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
            basket.ContactDetails = new ContactDetails
            {
                PeopleId = "5"
            };
            var responseBasket = await createHandler.Handle(new CreateBasketRequest() { Basket = basket }, CancellationToken.None);
        }

        [Fact]
        public async Task UpdateContactFailureWhenBasketIdMissingInCache()
        {
            Guid basketId = Guid.NewGuid();

            UpdateContactDetailsCommandHandler handler = new(_mockcontactLogger.Object, cache);

            var response = await handler.Handle(new UpdateContactDetailCommand() { Id = basketId }, CancellationToken.None);

            Assert.True(response.IsFailure);
        }

        [Fact]
        public async Task UpdateContactValidateEmailAddressReturnSuccesWithInValidStatus()
        {
            await PrepareBaseket();

            UpdateContactDetailsCommandHandler handler = new(_mockcontactLogger.Object, cache);

            var contact = new Domain.ContactDetails()
            {
                EmailAddress = "testFirstName.testLastName",
                PhoneNumber = "01234567892",
                Validation = "INVALID",
                CountryCode = "41",
                AlternativePhoneNumber = "1234567890",
                AcceptEmail = true,
                AcceptPost = false,
                AcceptTelephone = false,
                AcceptTextMessage = false
            };
            var response = await handler.Handle(new UpdateContactDetailCommand() { Id = basketId, ContactDetails = contact }, CancellationToken.None);

            var responseValue = response.Value.Value.ContactDetails.Validation;
            Assert.Equal("INVALID", responseValue);
        }


        [Fact]
        public async Task UpdateContactValidatePhoneNumberReturnSuccesWithInValidStatus()
        {
            await PrepareBaseket();

            UpdateContactDetailsCommandHandler handler = new(_mockcontactLogger.Object, cache);

            var contact = new Domain.ContactDetails()
            {
                EmailAddress = "testFirstName.testLastName",
                PhoneNumber = "231234567892",
                Validation = "INVALID",
                CountryCode = "41",
                AlternativePhoneNumber = "1234567890",
                AcceptEmail = true,
                AcceptPost = false,
                AcceptTelephone = false,
                AcceptTextMessage = false
            };
            var response = await handler.Handle(new UpdateContactDetailCommand() { Id = basketId, ContactDetails = contact }, CancellationToken.None);

            var responseValue = response.Value.Value.ContactDetails.Validation;
            Assert.Equal("INVALID", responseValue);
        }

        [Fact]
        public async Task UpdateContactValidateCountryCodeReturnSuccesWithInValidStatus()
        {
            await PrepareBaseket();

            UpdateContactDetailsCommandHandler handler = new(_mockcontactLogger.Object, cache);

            var contact = new Domain.ContactDetails()
            {
                EmailAddress = "testFirstName.testLastName",
                PhoneNumber = "01234567890",
                Validation = "INVALID",
                CountryCode = "",
                AlternativePhoneNumber = "1234567890",
                AcceptEmail = true,
                AcceptPost = false,
                AcceptTelephone = false,
                AcceptTextMessage = false
            };
            var response = await handler.Handle(new UpdateContactDetailCommand() { Id = basketId, ContactDetails = contact }, CancellationToken.None);

            var responseValue = response.Value.Value.ContactDetails.Validation;
            Assert.Equal("INVALID", responseValue);
        }

        [Fact]
        public async Task UpdateContactReturnSuccesWithValidStatus()
        {
            await PrepareBaseket();

            UpdateContactDetailsCommandHandler handler = new(_mockcontactLogger.Object, cache);

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
            var response = await handler.Handle(new UpdateContactDetailCommand() { Id = basketId, ContactDetails = contact }, CancellationToken.None);

            var responseValue = response.Value.Value.ContactDetails.Validation;
            Assert.Equal("VALID", responseValue);
            Assert.True(response.IsSuccess);

        }

        [Fact]
        public async Task UpdatePeopleIdReturnSuccesWithValidStatus()
        {
            await PrepareBaseket();

            UpdateContactDetailsCommandHandler handler = new(_mockcontactLogger.Object, cache);

            var contact = new Domain.ContactDetails()
            {
                EmailAddress = "testFirstName.testLastName@domain.com",
                PeopleId = "0",
                PhoneNumber = "11234567892",
                Validation = "INVALID",
                CountryCode = "41",
                AlternativePhoneNumber = "1234567890",
                AcceptEmail = true,
                AcceptPost = false,
                AcceptTelephone = false,
                AcceptTextMessage = false
            };
            var response = await handler.Handle(new UpdateContactDetailCommand() { Id = basketId, ContactDetails = contact }, CancellationToken.None);

            Assert.Equal("VALID", response.Value.Value.ContactDetails.Validation);
            Assert.Equal("0", response.Value.Value.ContactDetails.PeopleId);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task UpdatePeopleIdReturnSuccesWithInvalidStatus()
        {
            await PrepareBaseket();

            UpdateContactDetailsCommandHandler handler = new(_mockcontactLogger.Object, cache);

            var contact = new Domain.ContactDetails()
            {
                EmailAddress = "testFirstName.testLastName@domain.com",
                PeopleId = "1",
                PhoneNumber = "11234567892",
                Validation = "INVALID",
                CountryCode = "41",
                AlternativePhoneNumber = "1234567890",
                AcceptEmail = true,
                AcceptPost = false,
                AcceptTelephone = false,
                AcceptTextMessage = false
            };
            var response = await handler.Handle(new UpdateContactDetailCommand() { Id = basketId, ContactDetails = contact }, CancellationToken.None);

            Assert.Equal("INVALID", response.Value.Value.ContactDetails.Validation);
            Assert.Equal("1", response.Value.Value.ContactDetails.PeopleId);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task ClearPersonIdReturnSuccesWithValidStatus()
        {
            await PrepareBaseket();

            UpdateContactDetailsCommandHandler handler = new(_mockcontactLogger.Object, cache);

            var contact = new Domain.ContactDetails()
            {
                EmailAddress = "testFirstName.testLastName@domain.com",
                PeopleId = null,
                PhoneNumber = "11234567892",
                Validation = "INVALID",
                CountryCode = "41",
                AlternativePhoneNumber = "1234567890",
                AcceptEmail = true,
                AcceptPost = false,
                AcceptTelephone = false,
                AcceptTextMessage = false
            };
            var response = await handler.Handle(new UpdateContactDetailCommand() { Id = basketId, ContactDetails = contact }, CancellationToken.None);

            Assert.Equal("VALID", response.Value.Value.ContactDetails.Validation);
            Assert.Null(response.Value.Value.ContactDetails.PeopleId);
            Assert.True(response.IsSuccess);
        }
    }
}