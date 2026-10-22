using HotChocolate;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Mutations.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests.Mutation
{
    public class PassengerMutationsTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _httplogger;
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<PassengerMutations> _logger;
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IRefServiceProvider _refServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly ILogger<CustomerServiceProvider> _customerServiceProviderLogger;
        private readonly IInRuleServiceProvider _inRuleServiceProvider;
        private readonly IBasketEarnPointsCalculatorService _basketEarnPointsCalculatorService;

        private readonly IServiceProvider _serviceProvider;


        private readonly PassengerMutations _mutation;

        public PassengerMutationsTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("ReferenceMSCabinDetailsUrl", "https://Testresources.virginatlantic.com/SIT/ReferenceMicroservice/CabinDetails");
            ConfigReader.Set("ReferenceMSAirportDetailsUrl", "https://Testresources.virginatlantic.com/SIT/ReferenceMicroservice/airportdetail");
            ConfigReader.Set("ReferenceMSAirlineDetailsUrl", "https://Testresources.virginatlantic.com/SIT/ReferenceMicroservice/airlinedetail");
            ConfigReader.Set("Book:Offer:Url", "https://Testresources.virginatlantic.com/SIT/Offermicroservice/offers/{offerId}");
            ConfigReader.Set("book:offer:ocpsubid", "testvalue");
            ConfigReader.Set("book:offerms:channelid", "testvalue");
            ConfigReader.Set("referencemsocpkey", "testvalue");
            ConfigReader.Set("basketmsretrieveurl", "https://Testresources.virginatlantic.com/sit/basketmicroservice/basket/{basket}");
            ConfigReader.Set("BasketMSUpdatePassengerUrl", "https://Testresources.virginatlantic.com/sit/basketmicroservice/basket/{basket}/People/{peopleid}");
            ConfigReader.Set("BasketMSOcpKey", "testvalue");
            ConfigReader.Set("Identity:MsUrl", "https://Testresources.virginatlantic.com/sit/identitymicroservice");
            ConfigReader.Set("IdentityMS:AppId", "testid");
            ConfigReader.Set("Identity:OcpSubID", "testocpkey");
            ConfigReader.Set("Customer:UrlGetbyCustID", "https://Testresources.virginatlantic.com/dev/microservices/customer/{customerid}");
            ConfigReader.Set("Customer:UrlGetByMemNum", "https://Testresources.virginatlantic.com/dev/microservices/customer/loyalty/{memberid}");
            ConfigReader.Set("Customer:OcpSubID", "testocpkey");
            ConfigReader.Set("Customer:AppId", "testid");
            ConfigReader.Set("InRule:Url", "https://Testresources.virginatlantic.com/dev/inrule");
            ConfigReader.Set("InRule:OcpSubID", "testkey");
            ConfigReader.Set("InRule:AppId", "testappid");
            ConfigReader.Set("EAPIGraph:AppID", "testvalue");
            ConfigReader.Set("EAPIGraph:ClientSecret", "testsecret");
            ConfigReader.Set("AAD:Authority", "https://login.microsoftonline.com/");
            ConfigReader.Set("AAD:TenantId", "testvalue");

            var _tokenProvider = new Mock<IOAuthTokenProvider>();
            _tokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token

            _serviceProvider = new ServiceCollection().AddLogging()
                                          .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                          .AddSingleton<IHttpConnector, HttpConnector>()
                                          .AddSingleton<IBasketServiceProvider, BasketServiceProvider>()
                                          .AddSingleton<IOfferServiceProvider, OfferServiceProvider>()
                                          .AddSingleton<IRefServiceProvider, ReferenceServiceProvider>()
                                          .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
                                          .AddSingleton<ICustomerProvider, CustomerServiceProvider>()
                                          .AddSingleton<ICustomerServiceProvider, CustomerServiceProvider>()
                                          .AddSingleton<IInRuleServiceProvider, InRuleServiceProvider>()
                                          .AddSingleton<IBasketEarnPointsCalculatorService, BasketEarnPointsCalculatorService>()
                                          .AddSingleton(_tokenProvider.Object)
                                          .AddSingleton<PassengerMutations>()
                                          .BuildServiceProvider();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task UpdatePeople_ValidRequest_ReturnsUpdatedBasket()
        {
            var passengerMutations = _serviceProvider.GetService<PassengerMutations>();
            // Arrange
            var updateRequest = new PassengerUpdateRequest
            {
                BasketId = "36497a5d-e79a-4192-9495-1d09b1e135d2",
                People = new BasketPerson
                {
                    Id = "0",
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = Convert.ToDateTime("2000-01-21"),
                    Gender = "Male",
                    ProfilePassenger = false
                }
            };

            // Act
            var result = await passengerMutations.UpdatePeople(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("36497a5d-e79a-4192-9495-1d09b1e135d2", result.BasketId.ToString());
            Assert.NotNull(result.People);
            Assert.Equal(3, result.People.Count());

            var updatedPerson = result.People.FirstOrDefault(p => p.Id == "0");
            Assert.NotNull(updatedPerson);
            Assert.Equal("John", updatedPerson.FirstName);
            Assert.Equal("Doe", updatedPerson.LastName);
            Assert.Equal(
                Convert.ToDateTime("2000-01-21"),
                updatedPerson.DateOfBirth?.Date);

            Assert.Equal("VALID", updatedPerson.Validation);
            Assert.Equal("ADULT", updatedPerson.Type);

            // Membership validation
            Assert.NotNull(updatedPerson.MembershipDetails);
            Assert.Equal("VS", updatedPerson.MembershipDetails.FQTProgramme);
            Assert.Equal("9998887776", updatedPerson.MembershipDetails.FQTNumber);

            Assert.NotNull(result.OfferIds);
            Assert.Single(result.OfferIds);

            // Earn points assertions
            Assert.NotNull(result.EarnPoints);
            Assert.Equal(3020, result.EarnPoints.EarnVirginPoints);
            Assert.Equal(50, result.EarnPoints.EarnTierPoints);
            Assert.Equal(7430, result.EarnPoints.EarnGroupVirginPoints);
        }
    }
}