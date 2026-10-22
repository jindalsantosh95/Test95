using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Domain.Book.Mutations.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests.Mutation
{
    public class ContactDetailMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _httplogger;
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<ContactDetailMutation> _logger;
        private readonly IBasketServiceProvider _basketServiceProviderMock;
        private readonly IOfferServiceProvider _offerServiceProviderMock;
        private readonly IRefServiceProvider _refServiceProviderMock;
        private readonly ITokenB2CProvider<string> _tokenB2CProviderMock;
        private readonly ICustomerProvider _customerProviderMock;
        private readonly ICustomerServiceProvider _customerServiceProviderMock;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly ILogger<CustomerServiceProvider> _customerServiceProviderLogger;


        private readonly ContactDetailMutation _mutation;

        public ContactDetailMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("ReferenceMSCabinDetailsUrl", "https://Testresources.virginatlantic.com/SIT/ReferenceMicroservice/CabinDetails");
            ConfigReader.Set("ReferenceMSAirportDetailsUrl", "https://Testresources.virginatlantic.com/SIT/ReferenceMicroservice/airportdetail");
            ConfigReader.Set("ReferenceMSAirlineDetailsUrl", "https://Testresources.virginatlantic.com/SIT/ReferenceMicroservice/airlinedetail");
            ConfigReader.Set("Book:Offer:Url", "https://Testresources.virginatlantic.com/SIT/Offermicroservice/offers/{offerId}");
            ConfigReader.Set("book:offer:ocpsubid", "testvalue");
            ConfigReader.Set("book:offerms:channelid", "testvalue");
            ConfigReader.Set("referencemsocpkey", "testvalue");
            ConfigReader.Set("basketmsretrieveurl", "https://Testresources.virginatlantic.com/sit/basketmicroservice/basket/{basket}");
            ConfigReader.Set("BasketMSUpdateContactUrl", "https://Testresources.virginatlantic.com/sit/basketmicroservice/basket/{basket}/contact");
            ConfigReader.Set("BasketMSOcpKey", "testvalue");
            ConfigReader.Set("EnableAutoMatch", "true");
            ConfigReader.Set("BasketMSUpdatePassengerUrl", "https://Testresources.virginatlantic.com/sit/basketmicroservice/basket/{basket}/People/{peopleid}");


            _httplogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_httplogger, httpClientFactoryFixture.Current);
            _tokenProvider = new OAuthTokenProvider_AAD();
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ContactDetailMutation>.Instance;
            _customerServiceProviderLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomerServiceProvider>.Instance;
            _basketServiceProviderMock = new BasketServiceProvider(_httpConnector);
            _offerServiceProviderMock = new OfferServiceProvider(_httpConnector);
            _refServiceProviderMock = new ReferenceServiceProvider(_httpConnector);
            _tokenB2CProviderMock = new TokenB2CProvider<string>(_tokenProvider, _httpConnector);
            _customerProviderMock = new CustomerServiceProvider(_tokenProvider, _httpConnector, _customerServiceProviderLogger);
            _customerServiceProviderMock = new CustomerServiceProvider(_tokenProvider, _httpConnector, _customerServiceProviderLogger);

            _mutation = new ContactDetailMutation(
                _basketServiceProviderMock,
                _offerServiceProviderMock,
                _refServiceProviderMock,
                _tokenB2CProviderMock,
                _logger,
                _customerProviderMock,
                _customerServiceProviderMock
            );
            Setup.CreateTestEnvironmentSettings();
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task UpdateContactDetails_Should_Return_Basket_With_AutoMatch_Fields()
        {
            // Arrange
            Guid basketId = new("96497a5d-e79a-4192-9495-1d09b1e135d2");

            var contactUpdateRequest = new Basket
            {
                ContactDetails = new()
                {
                    EmailAddress = "latrinaselena@example.com",
                    PeopleId = "0",
                    PhoneNumber = "4045465466",
                    CountryCode = "44",
                    FcAutoMatchStamping = true
                },
            };

            
            // Act
            var result = await _mutation.UpdateContactDetails(new ContactUpdateRequest
            {
                BasketId = basketId.ToString(),
                ContactDetails = contactUpdateRequest.ContactDetails
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(basketId, result.BasketId);
            Assert.True(result.FcAutoMatched);
            Assert.True(result.ContactDetails.FcAutoMatchStamping);
            Assert.Equal("latrinaselena@example.com", result.ContactDetails.EmailAddress);
            Assert.Equal("0", result.ContactDetails.PeopleId);
        }
    }
}
