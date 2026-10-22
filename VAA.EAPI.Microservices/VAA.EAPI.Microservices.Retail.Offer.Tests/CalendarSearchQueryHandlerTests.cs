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
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.CalendarSearch;
using VAA.EAPI.Microservices.Retail.Offer.Domain.CalendarSearch;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOfferGql;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class CalendarSearchQueryHandlerTests
    {
        private readonly Mock<IGraphQLService> _graphqlService;
        private readonly DeltaGqlService _deltaGqlService;
        private readonly Mock<ILogger<IDeltaGqlService>> _logger;
        private readonly Mock<ILogger<CalendarSearchQueryHandler>> _logger2;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IAir4TokenGenerator> _air4TokenGenerator;
        private readonly CalendarSearchQueryHandler _handler;

        public CalendarSearchQueryHandlerTests()
        {
            _graphqlService = new Mock<IGraphQLService>();
            _logger = new Mock<ILogger<IDeltaGqlService>>();
            _logger2 = new Mock<ILogger<CalendarSearchQueryHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _air4TokenGenerator = new Mock<IAir4TokenGenerator>();
            _deltaGqlService = new DeltaGqlService(_graphqlService.Object, _logger.Object, _httpConnector.Object, _air4TokenGenerator.Object);
            _handler = new CalendarSearchQueryHandler(_deltaGqlService, _logger2.Object);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CalendarSearchQueryHandler(null, _logger2.Object));
            Assert.Throws<ArgumentNullException>(() => new CalendarSearchQueryHandler(_deltaGqlService, null));
        }

        [Fact]
        public async Task Handler_MixedAvailability()
        {
            //Arrange
            var request = new CalendarSearchQuery(TestDataProvider.GetTestData<CalendarOfferRequest>("OfferMS//MixedAvailability_CalendarOfferRQ"));
            var gqlResponse = TestDataProvider.GetTestData<GraphQLResponse>("OfferMS//MixedAvailability_DeltaGqlQueryRS");
            ConfigReader.Set(Constants.Environment.DlAwsOfferGqlUri, "not-in-test");
            ConfigReader.Set(Constants.Environment.DlAwsOfferClientId, "not-in-test");
            ConfigReader.Set(Constants.Environment.DlAwsOfferClientSecret, "not-in-test");
            ConfigReader.Set(Constants.Environment.DlApiApplicationId, "not-in-test");
            ConfigReader.Set(Constants.Environment.DlApiChannelId, "not-in-test");
            ConfigReader.Set(Constants.Environment.DlOfferPagingEnable, "false");
            ConfigReader.Set(Constants.Environment.IgnoreFlexFares, "true");

            _air4TokenGenerator.Setup(x => x.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult("not-in-test"));
            _graphqlService.Setup(x => x.SendAsync(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(gqlResponse));

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            
            Assert.Equal(new DateOnly(2025, 1, 5), result.Value.Value.Calendar.From);
            Assert.Equal(new DateOnly(2025, 2, 1), result.Value.Value.Calendar.To);

            // Check key values for a cheap day
            var seventh = result.Value.Value.Calendar.FromPrices.First(fp => fp.FromDate == new DateOnly(2025, 1, 7));
            Assert.Equal(18000, seventh.Price.AwardPoints);
            Assert.Equal(226.89m, seventh.Price.Amount);
            Assert.True(seventh.Price.MinimumPriceInMonth);
            Assert.True(seventh.Price.MinimumPriceInWeek);

            // Check key values for an expensive day
            var thirteenth = result.Value.Value.Calendar.FromPrices.First(fp => fp.FromDate == new DateOnly(2025, 1, 13));
            Assert.Equal(52000, thirteenth.Price.AwardPoints);
            Assert.Equal(164.29m, thirteenth.Price.Amount);
            Assert.False(thirteenth.Price.MinimumPriceInMonth);
            Assert.False(thirteenth.Price.MinimumPriceInWeek);

            // Check a sold out day is not included in results
            Assert.Equal(0, result.Value.Value.Calendar.FromPrices.Count(fp => fp.FromDate == new DateOnly(2025, 1, 17)));
        }
    }
}
