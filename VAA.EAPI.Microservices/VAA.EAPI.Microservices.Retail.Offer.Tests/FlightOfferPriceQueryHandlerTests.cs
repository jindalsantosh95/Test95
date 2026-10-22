using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Graphql;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOfferPrice;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta; 
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class OfferPriceQueryHandlerTests
    {
        private readonly Mock<ILogger<OfferPriceQueryHandler>> _logger;
        private readonly Mock<IDeltaGqlService> _deltaGqlService;
        private readonly OfferPriceQueryHandler _handler;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<IDistributedCacheService> _distributedCacheService;

        public OfferPriceQueryHandlerTests()
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();

            _logger = new Mock<ILogger<OfferPriceQueryHandler>>();
            _deltaGqlService = new Mock<IDeltaGqlService>();
            _mediator = new Mock<IMediator>();
            _distributedCacheService = new Mock<IDistributedCacheService>();
             _handler = new OfferPriceQueryHandler(_deltaGqlService.Object, _logger.Object, _mediator.Object, _distributedCacheService.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OfferPriceQueryHandler(null, _logger.Object, _mediator.Object, _distributedCacheService.Object));
            Assert.Throws<ArgumentNullException>(() => new OfferPriceQueryHandler(_deltaGqlService.Object, null, _mediator.Object, _distributedCacheService.Object));
            Assert.Throws<ArgumentNullException>(() => new OfferPriceQueryHandler(_deltaGqlService.Object, _logger.Object, null, _distributedCacheService.Object));
            Assert.Throws<ArgumentNullException>(() => new OfferPriceQueryHandler(_deltaGqlService.Object, _logger.Object, _mediator.Object, null));
        }

        [Fact]
        public async Task Handler_Revenue()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<FlightOfferRequest>("OfferMS/OfferPriceQuery/Revenue_OfferMsRQ");
            var cachedOffer = TestDataProvider.GetTestData<OfferDocument>("OfferMS/OfferPriceQuery/Revenue_DistributedCacheRS");
            var response = TestDataProvider.GetTestData<GraphQLResponse>("OfferMS/OfferPriceQuery/Revenue_DeltaGqlRS");

            _deltaGqlService.Setup(ds => ds.SendAsync(It.IsAny<GqlSearchOffersVariables>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>())).ReturnsAsync(response);
            _distributedCacheService.Setup(ds => ds.Get<OfferDocument>(It.Is<string>(s => s == "5985e907-ab8c-45ae-8c9a-9d0dbbd5cb12"))).ReturnsAsync(cachedOffer);

            // Act
            var result = await _handler.Handle(new OfferPriceQuery
            {
                Request = request,
                Type = Constants.Application.OfferPriceEventSubject
            }, CancellationToken.None);

            // Assert
            Assert.Equal(result.Value.Value.TotalOffers, result.Value.Value.Offers.Count());

            var selectedOffer = result.Value.Value.Offers.First(o => o.Selected);
            Assert.Equal("e44a1bce-79ba-4d5f-bd26-11611f119ef6-FL-1", selectedOffer.OfferId.Split("|")[1]);

            var price = selectedOffer.SalePrice;
            Assert.Equal("527.02", selectedOffer.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount);
            Assert.Equal("GBP", selectedOffer.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.BaseCurrency);
            
            var services = selectedOffer.OfferItems.First().Services;
            Assert.Equal("VSLT", services.First().FareDetails.DominantFareProduct);
            Assert.Equal(["FSEGVS19LHRSFO1"], services.First().FlightSegmentsRefIds);
            Assert.Equal(["FSEGVS20SFOLHR2"], services.Last().FlightSegmentsRefIds);

            var classicUpgrade = result.Value.Value.Offers.First(o => o.Selected == false && o.OfferItems.First().Services.First().FareDetails.DominantFareProduct == "VSCL");
            Assert.Equal("642.02", classicUpgrade.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount);

            var delightUpgrade = result.Value.Value.Offers.First(o => o.Selected == false && o.OfferItems.First().Services.First().FareDetails.DominantFareProduct == "VSDT");
            Assert.Equal("796.02", delightUpgrade.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount);
        }
    }
}
