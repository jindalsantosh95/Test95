using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Graphql;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOffer;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class FlightOfferQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OfferSearchRepoQueryHandler>> _logger;
        private readonly Mock<ILogger<DistributedCacheService>> _distributedCacheServiceLogger;
        private readonly Mock<IAir4TokenGenerator> _air4TokenGenerator;
        private readonly DistributedCacheService _distributedCacheService;
        private readonly FlightOfferQueryHandler _handler;

        public FlightOfferQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OfferSearchRepoQueryHandler>>();
            _distributedCacheServiceLogger = new Mock<ILogger<DistributedCacheService>>();
            _air4TokenGenerator = new Mock<IAir4TokenGenerator>();
            _distributedCacheService = new(new Core.Xunit.Caching.DistributedCache(), _distributedCacheServiceLogger.Object);
            _handler = new FlightOfferQueryHandler(_mediator.Object, _distributedCacheService);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenParamterIsNull()
        {
            Assert.Multiple(() => Assert.Throws<ArgumentNullException>(() => new FlightOfferQueryHandler(null, _distributedCacheService)),
                            () => Assert.Throws<ArgumentNullException>(() => new FlightOfferQueryHandler(_mediator.Object, null)));
        }

        [Fact]
        public async Task Handler_ForCityCodeSearch()
        {
            //Arrange
            ConfigReader.Set(Constants.Environment.SwitchToAwsGraphql, "true");
            ConfigReader.Set(Constants.Environment.DlAwsOfferGqlUri, "-");
            ConfigReader.Set(Constants.Environment.DlAwsOfferClientId, "-");
            ConfigReader.Set(Constants.Environment.DlAwsOfferClientSecret, "-");
            ConfigReader.Set(Constants.Environment.DlOfferPagingEnable, "false");
            ConfigReader.Set(Constants.Environment.DlApiApplicationId, "-");
            ConfigReader.Set(Constants.Environment.DlApiChannelId, "-");
            ConfigReader.Set(Constants.Environment.IgnoreFlexFares, "true");

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            _air4TokenGenerator.Setup(x => x.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult("not-in-test"));

            var request = new FlightOfferQuery
            {
                Request = TestDataProvider.GetTestData<FlightOfferRequest>("OfferMS/CityCode_OfferMsRQ"),
                Type = Constants.Application.FlightOfferEventSubject
            };
            var gqlResponse = TestDataProvider.GetTestData<GraphQLResponse>("OfferMS/CityCode_DeltaGqlQueryRS");

            var gqlService = new Mock<IGraphQLService>();
            gqlService.Setup(x => x.SendAsync(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(gqlResponse));

            var deltaGqlService = new DeltaGqlService(gqlService.Object, new Mock<ILogger<IDeltaGqlService>>().Object, new Mock<IHttpConnector>().Object, _air4TokenGenerator.Object);

            var distributedCacheServiceLogger = new Mock<ILogger<DistributedCacheService>>();
            var distributedCacheService = new DistributedCacheService(new Core.Xunit.Caching.DistributedCache(), distributedCacheServiceLogger.Object);

            var services = new ServiceCollection();
            var serviceProvider = services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblyContaining(typeof(FlightAvailabilityQuery));
                config.RegisterServicesFromAssemblyContaining(typeof(CosmosReaderServiceQueryHandler));
            })
                                          .AddSingleton(typeof(IAir4TokenGenerator), _air4TokenGenerator.Object)
                                          .AddSingleton(typeof(ILogger<FlightOfferServiceQueryHandler>), new Mock<ILogger<FlightOfferServiceQueryHandler>>().Object)
                                          .AddSingleton(typeof(ILogger<CosmosModifyServiceCommandHandler>), new Mock<ILogger<CosmosModifyServiceCommandHandler>>().Object)
                                          .AddSingleton(typeof(ILogger<CosmosModifyRepoCommandHandler>), new Mock<ILogger<CosmosModifyRepoCommandHandler>>().Object)
                                          .AddSingleton(typeof(IGraphQLService), gqlService.Object)
                                          .AddSingleton(typeof(IDeltaGqlService), deltaGqlService)
                                          .AddSingleton(typeof(IDistributedCacheService), distributedCacheService)
                                          .BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            //Act
            var result = await mediator.Send(request, CancellationToken.None);
            FlightOfferResponse flightOfferResponse = result.Value.Value;

            //Assert
            Assert.True(result.IsSuccess);

            // DataList is based on the airports flown, not the iata codes searched
            Assert.Contains(flightOfferResponse.DataList.OriginDestinationAssociations, od => od.OdLabel == "LHR-JFK");
            Assert.Contains(flightOfferResponse.DataList.OriginDestinationAssociations, od => od.OdLabel == "LGW-JFK");
            Assert.DoesNotContain(flightOfferResponse.DataList.OriginDestinationAssociations, od => od.OriginDestination.OriginAirport == "LON");
            Assert.DoesNotContain(flightOfferResponse.DataList.OriginDestinationAssociations, od => od.OriginDestination.OriginAirport == "NYC");

            // In the test response, VS flights have the promo code applied, while codeshare flights do not
            var vsOffer = flightOfferResponse.Offers.First(o => o.OfferItems.Any(oi => oi.Services.Any(s => s.FlightSegmentsRefIds?.Contains("FSEGVS3LHRJFK1") == true)));
            Assert.Equal("GB02", vsOffer.PromoCodeApplied);
            var codeshareOffer = flightOfferResponse.Offers.First(o => o.OfferItems.Any(oi => oi.Services.Any(s => s.FlightSegmentsRefIds?.Contains("FSEGVS7048LHRAMS12") == true)));
            Assert.Null(codeshareOffer.PromoCodeApplied);
        }
    }
}
