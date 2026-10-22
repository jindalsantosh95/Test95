using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reprice;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class OfferRepriceQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OfferRepriceQueryHandler>> _logger;
        private readonly Mock<ICabinDetails> _cabinDetails;
        private readonly OfferRepriceQueryHandler _handler;
        private readonly Mock<ICachedPriceClass> _cachedPriceClass;
        private readonly Mock<ICachedCabin> _cachedCabin;
        private readonly Mock<ICachedAmenities> _cachedAmenities;
        private readonly Mock<IFareFamily> _fareFamily;
        private readonly Mock<ICachedCabinBaggage> _cachedCabinBaggage;
        private readonly Mock<ICachedPieceAllowance> _cachedPieceAllowance;
        private readonly Mock<IDistributedCacheService> _distributedCacheService;

        public OfferRepriceQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OfferRepriceQueryHandler>>();
            _cabinDetails = new Mock<ICabinDetails>();
            _cachedCabin = new Mock<ICachedCabin>();
            _cachedAmenities = new Mock<ICachedAmenities>();
            _cachedPriceClass = new Mock<ICachedPriceClass>();
            _fareFamily = new Mock<IFareFamily>();
            _cachedCabinBaggage = new Mock<ICachedCabinBaggage>();
            _cachedPieceAllowance = new Mock<ICachedPieceAllowance>();
            _distributedCacheService = new Mock<IDistributedCacheService>();
            _handler = new OfferRepriceQueryHandler(_mediator.Object, _logger.Object, _cabinDetails.Object, _cachedPriceClass.Object, _cachedCabin.Object, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object);
            OfferTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OfferRepriceQueryHandler(null, _logger.Object, _cabinDetails.Object, _cachedPriceClass.Object, _cachedCabin.Object, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OfferRepriceQueryHandler(_mediator.Object, null, _cabinDetails.Object, _cachedPriceClass.Object, _cachedCabin.Object, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCabinObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new OfferRepriceQueryHandler(_mediator.Object, _logger.Object, null, _cachedPriceClass.Object, _cachedCabin.Object, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object));
        }
        [Fact]
        public async Task Handler_ShouldReturnResponse_WithGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OfferRepriceQuery>("OfferMS//OfferPriceRequest");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/OfferRepriceITAResponse.xml");
            XmlSerializer deserialiser = new(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OfferDocResponse");

            offerResponse.Offers.First().OfferExpirationTimeLimit = DateTime.UtcNow.AddMinutes(30).ToString(Constants.Application.ZuluDateTimeFormat);

            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerResponse);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerResponse);

            _cabinDetails.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );

            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Bussiness"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });
            _cachedAmenities.Setup(X => X.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(new List<AmenitiesTableEntity>()
            {
               new AmenitiesTableEntity{
                   RowKey = "DLDC01",
                   ProductComponentName="Operated by Delta Air Lines",
                   OperatingCarrier ="DL",
                   VAACabins = "Premium Economy",
                   PriceClass = "Premium Economy",
                   Cabins="Delta Comfort+"
               }
            });
            _cachedCabinBaggage.Setup(X => X.GetCachedCabinBaggageInformation(It.IsAny<string>())).ReturnsAsync(new List<CabinBaggageEntity>()
            {
               new CabinBaggageEntity{
                   RowKey = "CBO1",
                   AllowanceType="Dimension",
                   DimensionCategory ="Heiht",
                   TotalQuantity = "1",
                   Value = "55",
                   UnitCode="CM"
               }
            });
            _cachedPieceAllowance.Setup(X => X.GetCachedPieceAllowanceInformation(It.IsAny<string>())).ReturnsAsync(new List<PieceAllowanceTableEntity>()
            {
               new PieceAllowanceTableEntity{
                   RowKey = "HeihtCM",
                   DimensionCategory ="Height",
                   MaxMeasure = "55",
                   UnitCode="CM"
               }
            });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(2, result.Value.Value.DataList.CustomerDetails.Count);
            Assert.Equal(2, result.Value.Value.DataList.PriceClass.Count);

        }
    }
}
