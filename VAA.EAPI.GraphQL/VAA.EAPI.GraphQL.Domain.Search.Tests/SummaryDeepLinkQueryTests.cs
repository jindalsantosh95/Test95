using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Account;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.BasketMS;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.BrandMS;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.InRuleEngine;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.OfferMs;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.VRedMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public class SummaryDeepLinkQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public SummaryDeepLinkQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("refdatams:genericurl", "https://dummy.com/dev/ReferenceMicroservice/{0}");
            ConfigReader.Set("refdatams:ocpsubid", "dummykey");
            ConfigReader.Set("Search:BrandMS:URL", "https://dummy.com/brandmicroservice/brand");
            ConfigReader.Set("Search:BrandMS:OCPKey", "DummyKey");
            ConfigReader.Set("InRule:AppId", "DummyKey");
            ConfigReader.Set("InRule:URL", "https://vaa-test.com/inrule");
            ConfigReader.Set("InRule:OcpSubID", "DummyKey");
            ConfigReader.Set("Search:PostBackUrl", "https://dummy.com/sit/search");
            ConfigReader.Set("BasketMSCreateUrl", "https://dummy.com/basketmicroservice/basket");
            ConfigReader.Set("BasketMSOcpKey", "DummyKey");

            var _tokenProvider = new Mock<IOAuthTokenProvider>();
            _tokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IOfferPriceServices, OfferPriceServices>()
                                                      .AddSingleton<IOfferRetrieveService, OfferRetrieveService>()
                                                      .AddSingleton<IBasketService, BasketService>()
                                                      .AddSingleton<IBrandService, BrandService>()
                                                      .AddSingleton<IRefDataService<AircraftInfo>, RefDataService<AircraftInfo>>()
                                                      .AddSingleton<IRefDataService<AirportInfo>, RefDataService<AirportInfo>>()
                                                      .AddSingleton<IRefDataService<AirlineInfo>, RefDataService<AirlineInfo>>()
                                                      .AddSingleton<IRefDataService<CabinInfo>, RefDataService<CabinInfo>>()
                                                      .AddSingleton<IRefDataService<BaggageDetails>, RefDataService<BaggageDetails>>()
                                                      .AddSingleton<IRefDataService<TaxCodeInfo>, RefDataService<TaxCodeInfo>>()
                                                      .AddSingleton<IRefDataService<UpgradeInfo>, RefDataService<UpgradeInfo>>()
                                                      .AddSingleton<ITokenValidatorExtension, TokenValidatorExtension>()
                                                      .AddSingleton<IVRedServices, VredServices>()
                                                      .AddSingleton<IInRuleService, InRuleService>()
                                                      .AddSingleton<IVoucherRedemptionService, VoucherRedemptionService>()
                                                      .AddSingleton<ICustomerProvider, CustomerService>()
                                                      .AddSingleton<ICustomerTransactionProvider, CustomerTransactionService>()
                                                      .AddSingleton(typeof(ITokenB2CProvider<>), typeof(TokenB2CProvider<>))
                                                      .AddSingleton(_tokenProvider.Object)
                                                      .AddSingleton<TestableSummaryQuery>()
                                                      .BuildServiceProvider();
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task Return_LHRJFK()
        {
            var query = _serviceProvider.GetRequiredService<TestableSummaryQuery>();

            var basket = TestDataProvider.GetTestData<Basket>("Basket");
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");


            var result = await query.InvokeSearchSummaryShared(
                basket,
                false,
                "https://test/summary",
                Task.FromResult(offerPriceResponse),
                new() { Currency = "USD", Value = 500 }
            );

            Assert.NotNull(result);
            Assert.Equal("ECON-CLASSIC-FLEXIBLE", result.Upgrades.First().FareFamilyType);
            Assert.Equal("Economy Classic Flex", result.Upgrades.First().Content.CabinName);
            Assert.Equal("ECONOMY-DELIGHT", result.Upgrades.Last().FareFamilyType);
            Assert.Equal("Economy Delight", result.Upgrades.Last().Content.CabinName);

            // Check aircraft has been correctly mapped
            var aircraftservice = _serviceProvider.GetRequiredService<IRefDataService<AircraftInfo>>();
            var aircraftInfo = await aircraftservice.Query();
            Assert.Equal("Airbus A350-1000", result.Flights.First().Segments.First().Metal.First().Name);
            foreach (var segment in result.Flights.SelectMany(f => f.Segments))
            {
                var metal = segment.Metal.First();
                var offerMsFlightSegment = offerPriceResponse.DataList.FlightSegments.First(fs => segment.FlightNumber == fs.MarketingCarrier + fs.MarketingFlightNumber);
                var aircraft = aircraftInfo.FirstOrDefault(ai => ai.IataCode == offerMsFlightSegment.AirCraft);
                if (aircraft == null)
                {
                    // Falls back to the raw code returned from OfferMS
                    Assert.Equal(metal.Name, offerMsFlightSegment.AirCraft);
                }
                else
                {
                    Assert.Equal(metal.Name, aircraft.FullName);
                }

            }
        }
    }
}
