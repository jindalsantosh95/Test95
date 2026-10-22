using HotChocolate.Resolvers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Search.Helpers;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch;
using VAA.EAPI.GraphQL.Domain.Search.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public class SearchOffersTests
    {
        private readonly Mock<IOfferSearchServices> _offerSearchServices;
        private readonly Mock<IBasketService> _basketService;
        private readonly Mock<IRefDataService<AircraftInfo>> _aircraftInfoCacheService;
        private readonly Mock<IRefDataService<AirportInfo>> _airportInfoCacheService;
        private readonly Mock<IRefDataService<AirlineInfo>> _airlineInfoCacheService;
        private readonly Mock<IRefDataService<CabinInfo>> _cabinInfoCacheService;
        private readonly Mock<IResolverContext> _resolverContext;
        private readonly Mock<IGraphqlHelper> _graphqlHelper;
        private readonly Mock<TimeProvider> _timeProvider;
        private readonly Mock<ITokenValidatorExtension> _tokenValidatorExtension;
        private readonly Mock<IAnalyticsService> _analyticsService;

        public SearchOffersTests()
        {
            _offerSearchServices = new Mock<IOfferSearchServices>();
            _basketService = new Mock<IBasketService>();
            _aircraftInfoCacheService = new Mock<IRefDataService<AircraftInfo>>();
            _airportInfoCacheService = new Mock<IRefDataService<AirportInfo>>();
            _airlineInfoCacheService = new Mock<IRefDataService<AirlineInfo>>();
            _cabinInfoCacheService = new Mock<IRefDataService<CabinInfo>>();
            _resolverContext = new Mock<IResolverContext>();
            _graphqlHelper = new Mock<IGraphqlHelper>();
            _timeProvider = new Mock<TimeProvider>();
            _tokenValidatorExtension = new Mock<ITokenValidatorExtension>();
            _analyticsService = new Mock<IAnalyticsService>();

            var airportInfo = TestDataProvider.GetTestData<IEnumerable<AirportInfo>>("AirportInfo");
            _airportInfoCacheService.Setup(s => s.Query(It.IsAny<string[]>()))
                .ReturnsAsync((string[] airportCodes) => airportInfo.Where(ai => airportCodes.Contains(ai.AirportCode)));
            _airportInfoCacheService.Setup(s => s.Query()).ReturnsAsync(airportInfo);

            var aircraftInfo = TestDataProvider.GetTestData<IEnumerable<AircraftInfo>>("AircraftInfo");
            _aircraftInfoCacheService.Setup(s => s.Query()).ReturnsAsync(aircraftInfo);

            var cabinInfo = TestDataProvider.GetTestData<IEnumerable<CabinInfo>>("CabinInfo");
            _cabinInfoCacheService.Setup(s => s.Query()).ReturnsAsync(cabinInfo);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task OneWay_LHRATL()
        {
            var offerSearchBffQuery = new OfferSearchBFFQuery(
                _offerSearchServices.Object,
                _basketService.Object,
                _aircraftInfoCacheService.Object,
                _airportInfoCacheService.Object,
                _airlineInfoCacheService.Object,
                _cabinInfoCacheService.Object,
                _graphqlHelper.Object,
                _timeProvider.Object,
                _tokenValidatorExtension.Object,
                _analyticsService.Object
            );

            var request = TestDataProvider.GetTestData<FlightOfferRequest>("LHRATL_GqlRq");

            _basketService.Setup(s => s.CreateBasketAsync(It.IsAny<Basket>())).ReturnsAsync(new Basket { BasketId = Guid.NewGuid() });
            _timeProvider.Setup(t => t.GetUtcNow()).Returns(new DateTime(24, 3, 11));

            var offerMsResponse = TestDataProvider.GetTestData<FlightOfferResponse>("LHRATL_OfferMsRs");

            _offerSearchServices.Setup(s => s.FlightOfferSearchAsync(It.IsAny<FlightOfferRequest>())).ReturnsAsync(offerMsResponse);
            _graphqlHelper.Setup(s => s.GetSelections(It.IsAny<IResolverContext>())).Returns(new List<IFieldSelection> { });

            // Act
            var queryResponse = await offerSearchBffQuery.OfferSearch(request, _resolverContext.Object);

            // Assert
            Assert.NotNull(queryResponse.Result);

            // Check aircraft has been correctly mapped
            var aircraftInfo = await _aircraftInfoCacheService.Object.Query();
            Assert.Equal("Airbus A330-900neo", queryResponse.Result.Slice.FlightsAndFares.First().Flight.Segments.First().metal.First().name);
            foreach (var segment in queryResponse.Result.Slice.FlightsAndFares.Select(f => f.Flight).SelectMany(f => f.Segments))
            {
                var metal = segment.metal.First();
                var offerMsFlightSegment = offerMsResponse.DataList.FlightSegments.First(fs => segment.flightNumber == fs.MarketingCarrier + fs.MarketingFlightNumber);
                var aircraft = aircraftInfo.FirstOrDefault(ai => ai.IataCode == offerMsFlightSegment.AirCraft);
                if (aircraft == null)
                {
                    // Falls back to the raw code returned from OfferMS
                    Assert.Equal(metal.name, offerMsFlightSegment.AirCraft);
                }
                else
                {
                    Assert.Equal(metal.name, aircraft.FullName);
                }
            }

            // Check fare price has been correctly mapped
            Assert.Equal(1482, queryResponse.Result.Slice.FlightsAndFares.First().Fares.First().Price!.Amount);
            Assert.Equal(317.59m, queryResponse.Result.Slice.FlightsAndFares.First().Fares.First().Price!.Tax);
            Assert.Equal(1799.59m, queryResponse.Result.Slice.FlightsAndFares.First().Fares.First().Price!.AmountIncludingTax);
            foreach (var fare in queryResponse.Result.Slice.FlightsAndFares.SelectMany(f => f.Fares).Where(f => f.Price != null))
            {
                Assert.Equal(fare.Price!.AmountIncludingTax, fare.Price.Amount + fare.Price.Tax);
            }
        }

        public async Task Multidate_LHRJFK_BOSLHR()
        {
            var offerSearchBffQuery = new OfferSearchBFFQuery(
                _offerSearchServices.Object,
                _basketService.Object,
                _aircraftInfoCacheService.Object,
                _airportInfoCacheService.Object,
                _airlineInfoCacheService.Object,
                _cabinInfoCacheService.Object,
                _graphqlHelper.Object,
                _timeProvider.Object,
                _tokenValidatorExtension.Object,
                _analyticsService.Object
            );

            var request = TestDataProvider.GetTestData<FlightOfferRequest>("LHRJFK_BOSLHR_GqlRq");

            _basketService.Setup(s => s.CreateBasketAsync(It.IsAny<Basket>())).ReturnsAsync(new Basket { BasketId = Guid.NewGuid() });
            _timeProvider.Setup(t => t.GetUtcNow()).Returns(new DateTime(24, 3, 11));

            var flexiDateSearchResponse = TestDataProvider.GetTestData<FlexiDateSearchResponse>("LHRJFK_BOSLHR_FlexidateRs");
            _graphqlHelper.Setup(s => s.FieldIsSelected(It.IsAny<IReadOnlyList<IFieldSelection>>(), "priceGrid")).Returns(true);

            _offerSearchServices.Setup(s => s.FlightOfferSearchAsync(It.IsAny<FlightOfferRequest>())).ReturnsAsync(new FlightOfferResponse { });
            _offerSearchServices.Setup(s => s.FlexiDateSearchAsync(It.IsAny<FlexiDateOfferRequest>())).ReturnsAsync(flexiDateSearchResponse);
            _offerSearchServices.Setup(s => s.CalendarSearchAsync(It.IsAny<CalendarOfferRequest>())).ReturnsAsync(new CalendarSearchResponse { });

            // Act
            var queryResponse = await offerSearchBffQuery.OfferSearch(request, _resolverContext.Object);

            // Assert
            Assert.NotNull(queryResponse.PriceGrid);

            // Check all properties for first airport to ensure all properties are being populated
            Assert.Equal("Heathrow Airport", queryResponse.PriceGrid.Criteria.First().origin.AirportName);
            Assert.Equal("LHR", queryResponse.PriceGrid.Criteria.First().origin.Code);
            Assert.Equal("London", queryResponse.PriceGrid.Criteria.First().origin.CityName);
            Assert.Equal("United Kingdom", queryResponse.PriceGrid.Criteria.First().origin.CountryName);

            // Check a unique property for the remaining airports to ensure all airports are being populated
            Assert.Equal("John F Kennedy International", queryResponse.PriceGrid.Criteria.First().destination.AirportName);
            Assert.Equal("Logan International Airport", queryResponse.PriceGrid.Criteria.Last().origin.AirportName);
            Assert.Equal("Heathrow Airport", queryResponse.PriceGrid.Criteria.Last().destination.AirportName);
        }
    }
}
