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
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Search.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;
using BffSegment = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Segment;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public class SummaryQueryBaseTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<ITokenValidatorExtension> _tokenValidatorExtension = new();
        private readonly Mock<IVRedServices> _vRedServices = new();
        private readonly Mock<ICustomerProvider> _customerProvider = new();
        private readonly Mock<ICustomerTransactionProvider> _customerTransactionProvider = new();
        private readonly Mock<IBasketService> _basketService = new();

        public SummaryQueryBaseTests(HttpClientFactoryFixture httpClientFactoryFixture)
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
                                                      .AddSingleton(_basketService.Object)
                                                      .AddSingleton<IBrandService, BrandService>()
                                                      .AddSingleton<IRefDataService<AircraftInfo>, RefDataService<AircraftInfo>>()
                                                      .AddSingleton<IRefDataService<AirportInfo>, RefDataService<AirportInfo>>()
                                                      .AddSingleton<IRefDataService<AirlineInfo>, RefDataService<AirlineInfo>>()
                                                      .AddSingleton<IRefDataService<CabinInfo>, RefDataService<CabinInfo>>()
                                                      .AddSingleton<IRefDataService<BaggageDetails>, RefDataService<BaggageDetails>>()
                                                      .AddSingleton<IRefDataService<TaxCodeInfo>, RefDataService<TaxCodeInfo>>()
                                                      .AddSingleton<IRefDataService<UpgradeInfo>, RefDataService<UpgradeInfo>>()
                                                      .AddSingleton(_tokenValidatorExtension.Object)
                                                      .AddSingleton(_vRedServices.Object)
                                                      .AddSingleton<IInRuleService, InRuleService>()
                                                      .AddSingleton<IVoucherRedemptionService, VoucherRedemptionService>()
                                                      .AddSingleton(_customerProvider.Object)
                                                      .AddSingleton(typeof(ITokenB2CProvider<>), typeof(TokenB2CProvider<>))
                                                      .AddSingleton(_tokenProvider.Object)
                                                      .AddSingleton(_customerTransactionProvider.Object)
                                                      .AddSingleton<TestableSummaryQuery>()
                                                      .BuildServiceProvider();
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void IsDirectVirginAtlanticOneWay_ReturnsTrue_ForOneNonStopVirginAtlanticSector()
        {
            var result = SummaryQueryBase.IsDirectVirginAtlanticOneWay(
                originDestinationCount: 1,
                flights:
                [
                    new Flight
                    {
                        Segments = [CreateBffSegment("3")]
                    }
                ]);

            Assert.True(result);
        }

        [Fact]
        public void IsDirectVirginAtlanticOneWay_ReturnsFalse_ForAConnectingJourney()
        {
            var result = SummaryQueryBase.IsDirectVirginAtlanticOneWay(
                originDestinationCount: 1,
                flights:
                [
                    new Flight
                    {
                        Segments =
                        [
                            CreateBffSegment("3"),
                            CreateBffSegment("4")
                        ]
                    }
                ]);

            Assert.False(result);
        }

        private static BffSegment CreateBffSegment(string operatingFlightNumber)
        {
            var offer = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var segment = offer.DataList.FlightSegments.First();
            segment.MarketingCarrier = "VS";
            segment.MarketingFlightNumber = operatingFlightNumber;
            segment.OperatingCarrier = "VS";
            segment.OperatingFlightNumber = operatingFlightNumber;
            segment.ConnectionOrStopOver = "0";

            return new BffSegment(
                segment,
                offer.Offers.First().OfferItems.First().Services.First(),
                stopCount: 0,
                cabinInfo: Enumerable.Empty<CabinInfo>(),
                aircraftInfo: Enumerable.Empty<AircraftInfo>(),
                airlineInfo: [new AirlineInfo { CarrierCode = "VS", CarrierName = "Virgin Atlantic" }],
                airportInfo: Enumerable.Empty<AirportInfo>());
        }

        [Fact]
        public async Task SearchSummary_ReturnsExpectedSummary()
        {
            // Arrange
            var summaryQuery = _serviceProvider.GetService<TestableSummaryQuery>();

            var basket = TestDataProvider.GetTestData<Models.Basket.Basket>("Basket");
            var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

            var summaryResponse = await summaryQuery.InvokeSearchSummaryShared(basket, false, "https://vaa-test.com/flights/search/summary", Task.FromResult(offerPriceResponse), null);

            // Assert - Basic
            Assert.NotNull(summaryResponse);
            Assert.NotNull(summaryResponse.Price);
            Assert.NotNull(summaryResponse.Flights);
            Assert.Single(summaryResponse.Flights);

            // Assert - Price
            Assert.Equal(428.00m, summaryResponse.Price.Amount);
            Assert.Equal("GBP", summaryResponse.Price.Currency);
            Assert.Equal(337.59m, summaryResponse.Price.Tax);
            Assert.Equal(765.59m, summaryResponse.Price.AmountIncludingTax);

            // Assert - Flight
            var flight = summaryResponse.Flights.ElementAt(0);
            Assert.Equal("LHR", flight.Origin.Code);
            Assert.Equal("JFK", flight.Destination.Code);
            Assert.Equal("PT08H10M", flight.Duration);
            Assert.Equal("Economy Classic", flight.CabinName);
            Assert.Equal("ECON", flight.FareFamilyType);

            // Assert - Segment
            var segment = flight.Segments.ElementAt(0);
            Assert.Equal("VS", segment.Airline.Code);
            Assert.Equal("VS3", segment.FlightNumber);
            Assert.Equal("LHR", segment.Origin.Code);
            Assert.Equal("JFK", segment.Destination.Code);

            // Assert - Baggage
            var baggage = summaryResponse.FlightPassengerBaggage.First();

            Assert.Equal(1, baggage.CarryOnBag.PassengerBaggageAllowance.FreeBaggageAllowance.AllowedBagCnt);
            Assert.Equal(10, baggage.CarryOnBag.PassengerBaggageAllowance.FreeBaggageAllowance.Weight.UnitOfMeasureCnt);

            Assert.Equal(1, baggage.CheckedBag.PassengerBaggageAllowance.FreeBaggageAllowance.AllowedBagCnt);
            Assert.Equal(23, baggage.CheckedBag.PassengerBaggageAllowance.FreeBaggageAllowance.Weight.UnitOfMeasureCnt);

            // Assert - Earn Points
            Assert.Equal("1720", summaryResponse.EarnPoints.GroupVirginPoints);
            Assert.Equal("25", summaryResponse.EarnPoints.TierPoints);


        }

        [Fact]
        public async Task SearchSummary_DoesNotEnableVoucherRedemption_WhenOperatingCarrierIsNotVS()
        {
            ConfigReader.Set(Constants.Environment.EnableVoucherRedemption, "true");

            try
            {
                var summaryQuery = _serviceProvider.GetService<TestableSummaryQuery>();
                var basket = TestDataProvider.GetTestData<Models.Basket.Basket>("Basket");
                var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
                offerPriceResponse.DataList.FlightSegments.First().OperatingCarrier = "DL";

                var summaryResponse = await summaryQuery.InvokeSearchSummaryShared(
                    basket,
                    isAwardSearch: true,
                    null,
                    Task.FromResult(offerPriceResponse),
                    null);

                Assert.NotNull(summaryResponse);
                Assert.Null(summaryResponse.VoucherDetails);
            }
            finally
            {
                ConfigReader.Set(Constants.Environment.EnableVoucherRedemption, "false");
            }
        }

        [Fact]
        public async Task SearchSummary_EnablesVoucherRedemption_WhenAllOperatingCarriersAreVS()
        {
            ConfigReader.Set(Constants.Environment.EnableVoucherRedemption, "true");

            try
            {
                var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

                Assert.True(SummaryQueryBase.IsVoucherRedemptionEnabled(true, offerPriceResponse));
            }
            finally
            {
                ConfigReader.Set(Constants.Environment.EnableVoucherRedemption, "false");
            }
        }

        [Fact]
        public async Task SearchSummary_ReturnsVoucherExpiryEligibility_WhenOutboundTravelIsAfterVoucherExpiry()
        {
            ConfigReader.Set(Constants.Environment.EnableVoucherRedemption, "true");

            try
            {
                var today = DateTime.Today;
                _tokenValidatorExtension
                    .Setup(service => service.ValidateTokenExtractFCNumber(It.IsAny<string>()))
                    .ReturnsAsync("1234567890");
                _vRedServices
                    .Setup(service => service.GetVRedPoints(It.IsAny<string>()))
                    .ReturnsAsync(new VRedResponse());
                _customerProvider
                    .Setup(service => service.GetCustomer(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new CustomerDTO { Customer = new CustomerCustMS { CustomerId = "customer-id" } });
                _customerTransactionProvider
                    .Setup(service => service.RetrieveVoucher("customer-id"))
                    .ReturnsAsync([
                        new VoucherDetail
                        {
                            VoucherNumber = "expired-voucher",
                            Status = "Available",
                            ExpiryDate = today.AddDays(3)
                        }
                    ]);

                var summaryQuery = _serviceProvider.GetRequiredService<TestableSummaryQuery>();
                var basket = TestDataProvider.GetTestData<Models.Basket.Basket>("Basket");
                basket.FlightsearchRequest.SearchOriginDestinations.First().DepartureDate = today.AddDays(5).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                var offerPriceResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");

                var summaryResponse = await summaryQuery.InvokeSearchSummaryShared(
                    basket,
                    isAwardSearch: true,
                    null,
                    Task.FromResult(offerPriceResponse),
                    null);

                Assert.NotNull(summaryResponse.VoucherEligibility);
                Assert.False(summaryResponse.VoucherEligibility.Eligible);
                Assert.Equal("VOUCHER_EXPIRES_BEFORE_OUTBOUND", summaryResponse.VoucherEligibility.ReasonCode);
                Assert.NotNull(summaryResponse.VoucherDetails);
                Assert.Empty(summaryResponse.VoucherDetails.AvailableVouchers);
            }
            finally
            {
                ConfigReader.Set(Constants.Environment.EnableVoucherRedemption, "false");
            }
        }

        [Fact]
        public void VoucherRedemption_UsesOnlyTheTwoEarliestExpiringEligibleVouchers()
        {
            var vouchers = new[]
            {
                new VoucherDetail { VoucherNumber = "third", ExpiryDate = new DateTime(2028, 9, 4) },
                new VoucherDetail { VoucherNumber = "first", ExpiryDate = new DateTime(2028, 8, 31) },
                new VoucherDetail { VoucherNumber = "second", ExpiryDate = new DateTime(2028, 9, 1) }
            };

            var selectedVouchers = SummaryQueryBase.GetEarliestExpiringVouchers(vouchers);

            Assert.Equal(["first", "second"], selectedVouchers.Select(voucher => voucher.VoucherNumber));
        }

        [Fact]
        public void VoucherRedemption_RetainsAllEligibleVouchers_WhenThereAreAtMostTwo()
        {
            var vouchers = new[]
            {
                new VoucherDetail { VoucherNumber = "second", ExpiryDate = new DateTime(2028, 9, 1) },
                new VoucherDetail { VoucherNumber = "first", ExpiryDate = new DateTime(2028, 8, 31) }
            };

            var selectedVouchers = SummaryQueryBase.GetEarliestExpiringVouchers(vouchers);

            Assert.Equal(["first", "second"], selectedVouchers.Select(voucher => voucher.VoucherNumber));
        }
    }
}
