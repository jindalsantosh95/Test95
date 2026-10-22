using HotChocolate;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Queries;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests.Queries
{
    public class RetrieveBasketQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public RetrieveBasketQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("ReferenceMSCabinDetailsUrl", "https://Testresources.virginatlantic.com/SIT/ReferenceMicroservice/CabinDetails");
            ConfigReader.Set("ReferenceMSAirportDetailsUrl", "https://Testresources.virginatlantic.com/SIT/ReferenceMicroservice/airportdetail");
            ConfigReader.Set("ReferenceMSAirlineDetailsUrl", "https://Testresources.virginatlantic.com/SIT/ReferenceMicroservice/airlinedetail");

            ConfigReader.Set("Book:Offer:Url", "https://Testresources.virginatlantic.com/SIT/Offermicroservice/offers/{offerId}");
            ConfigReader.Set("book:offer:ocpsubid", "testvalue");
            ConfigReader.Set("book:offerms:channelid", "testvalue");

            ConfigReader.Set("referencemsocpkey", "testvalue");

            ConfigReader.Set("basketmsretrieveurl", "https://Testresources.virginatlantic.com/sit/basketmicroservice/basket/{basket}");
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

            var tokenProvider = new Mock<IOAuthTokenProvider>();
            tokenProvider
                .Setup(m => m.AcquireTokenAsync(It.IsAny<string>()))
                .ReturnsAsync("abc");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IBasketServiceProvider, BasketServiceProvider>()
                .AddSingleton<IOfferServiceProvider, OfferServiceProvider>()
                .AddSingleton<IRefServiceProvider, ReferenceServiceProvider>()
                .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
                .AddSingleton<ICustomerProvider, CustomerServiceProvider>()
                .AddSingleton<ICustomerServiceProvider, CustomerServiceProvider>()
                .AddSingleton<IInRuleServiceProvider, InRuleServiceProvider>()
                .AddSingleton<IVRedServiceProvider, VRedServiceProvider>()
                .AddSingleton<IBasketEarnPointsCalculatorService, BasketEarnPointsCalculatorService>()
                .AddSingleton(tokenProvider.Object)
                .AddSingleton<RetrieveBasketQuery>()
                .BuildServiceProvider();


            AsyncContext.Current = new AsyncContext()
            {
                Context = new TestFunctionContext()
            };
        }

        [Fact]
        public async Task RetrieveBasket_ValidBasketId_ReturnsBasketWithCorrectProperties()
        {
            var retrieveBasketQuery = _serviceProvider.GetRequiredService<RetrieveBasketQuery>();
            // Arrange
            var basketId = "36497a5d-e79a-4192-9495-1d09b1e135d2";

            // Act
            var result = await retrieveBasketQuery.RetrieveBasket(basketId);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(
                basketId,
                result.BasketId.ToString());

            Assert.NotNull(result.OfferIds);
            Assert.Single(result.OfferIds);
            Assert.NotNull(result.Summary);
            Assert.NotEmpty(result.InsuranceQuotes);

            Assert.NotNull(result.People);
            var passenger = result.People.First();
            Assert.Equal("John", passenger.FirstName);

            Assert.NotNull(result.EarnPoints);
            Assert.Equal(3020, result.EarnPoints.EarnVirginPoints);
            Assert.Equal(50, result.EarnPoints.EarnTierPoints);
            Assert.Equal(7430, result.EarnPoints.EarnGroupVirginPoints);
        }

        [Fact]
        public async Task RetrieveBasket_InvalidBasketId_ThrowsGraphQLException()
        {
            var retrieveBasketQuery = _serviceProvider.GetRequiredService<RetrieveBasketQuery>();

            // Arrange
            string basketId = null;

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(
                () => retrieveBasketQuery.RetrieveBasket(basketId));
        }
    }
}