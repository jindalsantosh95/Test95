using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Graphql;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOfferGql;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class FlightOfferGqlServiceQueryHandlerTests
    {
        private readonly Mock<IGraphQLService> _graphqlService;
        private readonly DeltaGqlService _deltaGqlService;
        private readonly Mock<ILogger<IDeltaGqlService>> _logger;
        private readonly Mock<ILogger<FlightOfferServiceQueryHandler>> _logger2;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IAir4TokenGenerator> _air4TokenGenerator;
        private readonly FlightOfferGqlServiceQueryHandler _handler;

        public FlightOfferGqlServiceQueryHandlerTests()
        {
            _graphqlService = new Mock<IGraphQLService>();
            _logger = new Mock<ILogger<IDeltaGqlService>>();
            _logger2 = new Mock<ILogger<FlightOfferServiceQueryHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _air4TokenGenerator = new Mock<IAir4TokenGenerator>();
            _deltaGqlService = new DeltaGqlService(_graphqlService.Object, _logger.Object, _httpConnector.Object, _air4TokenGenerator.Object);
            _handler = new FlightOfferGqlServiceQueryHandler(_deltaGqlService, _logger2.Object);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlightOfferGqlServiceQueryHandler(null, _logger2.Object));
            Assert.Throws<ArgumentNullException>(() => new FlightOfferGqlServiceQueryHandler(_deltaGqlService, null));
        }

        [Fact]
        public async Task Handler_Reward_Saver()
        {
            //Arrange
            var request = new FlightOfferGqlServiceQuery
            {
                Request = TestDataProvider.GetTestData<FlightOfferRequest>("OfferMS//Reward_Saver_OfferSearchRQ")
            };
            var gqlResponse = TestDataProvider.GetTestData<GraphQLResponse>("OfferMS//Reward_Saver_DeltaGqlQueryRS");
            ConfigReader.Set(Constants.Environment.DlAwsOfferGqlUri, "not-in-test");
            ConfigReader.Set(Constants.Environment.DlAwsOfferClientId, "not-in-test");
            ConfigReader.Set(Constants.Environment.DlApiApplicationId, "not-in-test");
            ConfigReader.Set(Constants.Environment.DlApiChannelId, "not-in-test");
            ConfigReader.Set(Constants.Environment.DlOfferPagingEnable, "false");
            ConfigReader.Set(Constants.Environment.IgnoreFlexFares, "true");
            ConfigReader.Set(Constants.Environment.DlAwsOfferClientSecret, "not-in-test");

            _air4TokenGenerator.Setup(x => x.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult("not-in-test"));
            _graphqlService.Setup(x => x.SendAsync(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(gqlResponse));

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Status);

            var Vs453DirectOffers = result.Value.Offers.Where(o =>
                o.OfferItems.First().Services.First().FlightSegmentsRefIds.Count() == 1
                && o.OfferItems.First().Services.First().FlightSegmentsRefIds.Contains("FSEGVS453LHRTLV1"));
            // Ensorsements are LON-TLV AFM02 (non-saver), TLV-LON ""
            Assert.False(Vs453DirectOffers.First(o => o.OfferItems.First().Services.First().FareDetails.DominantFareProduct == "VSCL").IsSaverFare, "Incorrect IsSaverFare: FSEGVS453LHRTLV1 in VSCL does not contain any saver segments");
            // Endorsements are LON-TLV AMF06 (saver), TLV-LON AMF02 (non-saver)
            Assert.True(Vs453DirectOffers.First(o => o.OfferItems.First().Services.First().FareDetails.DominantFareProduct == "VSPE").IsSaverFare, "Incorrect IsSaverFare: FSEGVS453LHRTLV1 in VSPE has a saver fare on outbound journey");
            //  Endorsements are LON-TLV AFM02 (non-saver), TLV-PAR "", PAR-LON ""
            Assert.False(Vs453DirectOffers.First(o => o.OfferItems.First().Services.First().FareDetails.DominantFareProduct == "VSUP").IsSaverFare, "Incorrect IsSaverFare: FSEGVS453LHRTLV1 in VSUP does not contain any saver segments");

            var klConnectingOffers = result.Value.Offers.Where(o =>
                o.OfferItems.First().Services.First().FlightSegmentsRefIds.Count() == 2
                && o.OfferItems.First().Services.First().FlightSegmentsRefIds.Contains("FSEGVS1012LHRAMS2")
                && o.OfferItems.First().Services.First().FlightSegmentsRefIds.Contains("FSEGVS461AMSTLV2"));
            //  Endorsements are LON-TLV "", TLV-LON "". Flight changes in AMS in both directions.
            Assert.False(klConnectingOffers.First(o => o.OfferItems.First().Services.First().FareDetails.DominantFareProduct == "KLBU").IsSaverFare, "Incorrect IsSaverFare: VS1012LHRAMS2+VS461AMSTLV2 in KLBU does not contain any saver segments");
        }
    }
}
