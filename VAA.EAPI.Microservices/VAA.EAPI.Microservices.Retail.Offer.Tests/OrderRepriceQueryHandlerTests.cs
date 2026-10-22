using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reprice;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class OrderRepriceQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderRepriceQueryHandler>> _logger;
        private readonly Mock<ICabinDetails> _cabinDetail;
        private readonly Mock<ICachedPriceClass> _cachedPriceClass;
        private readonly Mock<ICachedCabin> _cachedCabin;
        private readonly Mock<ICachedAmenities> _cachedAmenities;
        private readonly Mock<IFareFamily> _fareFamily;
        private readonly OrderRepriceQueryHandler _handler;
        private readonly Mock<ICachedCabinBaggage> _cachedCabinBaggage;
        private readonly Mock<ICachedPieceAllowance> _cachedPieceAllowance;
        private readonly Mock<IDistributedCacheService> _distributedCacheService;


        public OrderRepriceQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderRepriceQueryHandler>>();
            _cabinDetail = new Mock<ICabinDetails>();
            _cachedCabin = new Mock<ICachedCabin>();
            _cachedAmenities = new Mock<ICachedAmenities>();
            _cachedPriceClass = new Mock<ICachedPriceClass>();
            _fareFamily = new Mock<IFareFamily>();
            _cachedCabinBaggage = new Mock<ICachedCabinBaggage>();
            _cachedPieceAllowance = new Mock<ICachedPieceAllowance>();
            _distributedCacheService = new Mock<IDistributedCacheService>();
            _handler = new OrderRepriceQueryHandler(_mediator.Object, _logger.Object, _cabinDetail.Object, _cachedPriceClass.Object, _cachedCabin.Object, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object);
            OfferTestsCommon.CreateTestEnvironmentSettings();

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

        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderRepriceQueryHandler(null, _logger.Object, _cabinDetail.Object, _cachedPriceClass.Object, _cachedCabin.Object, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderRepriceQueryHandler(_mediator.Object, null, _cabinDetail.Object, _cachedPriceClass.Object, _cachedCabin.Object, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCabinObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderRepriceQueryHandler(_mediator.Object, _logger.Object, null, _cachedPriceClass.Object, _cachedCabin.Object, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCachedCabinObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderRepriceQueryHandler(_mediator.Object, _logger.Object, _cabinDetail.Object, _cachedPriceClass.Object, null, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCachedPriceClassObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderRepriceQueryHandler(_mediator.Object, _logger.Object, _cabinDetail.Object, null, _cachedCabin.Object, _cachedAmenities.Object, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCachedAmenitiesObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderRepriceQueryHandler(_mediator.Object, _logger.Object, _cabinDetail.Object, _cachedPriceClass.Object, _cachedCabin.Object, null, _fareFamily.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _distributedCacheService.Object));
        }

        [Fact]
        public async Task Handler_ShouldSucceed_WithGoodRequest()
        {
            //Arrang
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponse");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");

            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
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
            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);
        }
        [Fact]
        public async Task Handler_FailureResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceMandatoryMissing");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponse");

            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
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
                ProductName = "Economy Classic Flex"
            });

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);
            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.ShoppingRefIDMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.ShoppingRefIDMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OfferRefIDMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OfferRefIDMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.ServiceBeneficiariesMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.ServiceBeneficiariesMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.CustIdMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.CustIdMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.PtcMissingorInvalid));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.PtcMissingorInvalid).FirstOrDefault()?.Message);
        }
        [Fact]
        public async Task Handler_ShouldFail_WithBadRequest()
        {
            //Arrang
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailswithError");

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
        }
        [Fact]
        public async Task Handler_ShouldRetrunCustomerDetailsandBeneficiaries_WithGoodRequest()
        {
            //Arrang
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            request.CustomerDetails.ForEach(x => x.Ptc = "ADT");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponse");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse")
;
            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
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
                   PriceClass = "Economy Delight",
                   Cabins="Delta Comfort+"
               }
            });

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("9999508066157", result.Value.Value.DataList.CustomerDetails.First().CustId);
            Assert.Equal("ADT", result.Value.Value.DataList.CustomerDetails.First().Ptc);
            Assert.Equal(new List<string> { "9999508066157" }, result.Value.Value.Offers.First().OfferItems.First().Services.First().ServiceBeneficiaries);
        }
        [Fact]
        public async Task Handler_ShouldRetrunFareandTaxes_ForSingleSegmentSinglePTC()
        {
            //Arrang
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            request.CustomerDetails.ForEach(x => x.Ptc = "ADT");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponse");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");

            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );
            _cachedAmenities.Setup(X => X.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(new List<AmenitiesTableEntity>()
            {
               new AmenitiesTableEntity{
                   RowKey = "DLDC01",
                   ProductComponentName="Operated by Delta Air Lines",
                   OperatingCarrier ="DL",
                   VAACabins = "Premium Economy",
                   PriceClass = "Economy Delight Flex",
                   Cabins="Delta Comfort+"
               }
            });
            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value.Offers.First().SalePrice.First().Amount);
            Assert.NotNull(result.Value.Value.Offers.Last().SalePriceDifferences.Last().Amount.MonetaryAmount);
        }
        [Fact]
        public async Task Handler_ShouldRetrunFareandTaxes_ForSingleSegmentMultiPTC()
        {
            //Arrang
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequestWithMultiPTC");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponse");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");

            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );
            _cachedAmenities.Setup(X => X.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(new List<AmenitiesTableEntity>()
            {
               new AmenitiesTableEntity{
                   RowKey = "DLDC01",
                   ProductComponentName="Operated by Delta Air Lines",
                   OperatingCarrier ="DL",
                   VAACabins = "Premium Economy",
                   PriceClass = "Economy Delight Flex",
                   Cabins="Delta Comfort+"
               }
            });

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value.Offers.First().SalePrice.First().Amount);
            Assert.NotNull(result.Value.Value.Offers.Last().SalePriceDifferences.First().Amount.MonetaryAmount);
        }
        [Fact]
        public async Task Handler_ShouldRetrunFareandTaxes_ForMultiSegmentSinglePTC()
        {
            //Arrang
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            request.CustomerDetails.ForEach(x => x.Ptc = "ADT");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponseForMultiSegment");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");

            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );
            _cachedAmenities.Setup(X => X.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(new List<AmenitiesTableEntity>()
            {
               new AmenitiesTableEntity{
                   RowKey = "DLDC01",
                   ProductComponentName="Operated by Delta Air Lines",
                   OperatingCarrier ="DL",
                   VAACabins = "Premium Economy",
                   PriceClass = "Economy Delight",
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

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value.Offers.First().SalePrice.First().Amount);
            Assert.NotNull(result.Value.Value.Offers.Last().SalePriceDifferences.Last().Amount.MonetaryAmount);
        }
        [Fact]
        public async Task Handler_ShouldRetrunFareandTaxes_ForMultiSegmentMultiPTC()
        {
            //Arrang
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequestWithMultiPTC");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponseForMultiSegment");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");

            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );
            _cachedAmenities.Setup(X => X.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(new List<AmenitiesTableEntity>()
            {
               new AmenitiesTableEntity{
                   RowKey = "DLDC01",
                   ProductComponentName="Operated by Delta Air Lines",
                   OperatingCarrier ="DL",
                   VAACabins = "Premium Economy",
                   PriceClass = "Economy Delight",
                   Cabins="Delta Comfort+"
               }
            });

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value.Offers.First().SalePrice.First().Amount);
            Assert.NotNull(result.Value.Value.Offers.Last().SalePriceDifferences.First().Amount.MonetaryAmount);

        }

        [Fact]
        public async Task Handler_ShouldReturnBaggageInfoInResponse_ForSingleSegmentSinglePTC()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponseForSingleSegmentSinglePtc");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");

            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(5, result.Value.Value.DataList.BaggageAllowances.Count);
            Assert.Contains("BAG1", result.Value.Value.Offers.First(x => x.Selected == true).OfferItems.First().Services.First().BaggageRefIds);
            Assert.Contains("BAG2", result.Value.Value.Offers.First(x => x.Selected == true).OfferItems.First().Services.First().BaggageRefIds);

        }

        [Fact]
        public async Task Handler_ShouldReturnBaggageInfoResponse_ForSingleSegmentMultiPTCOneAdultOneYPOneChildOneInfant()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequestWithSinglePassengerForAllPtcs");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponseForSingleSegmentSinglePassengerForAllPtcs");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");
            var brandDetails = TestDataProvider.GetTestData<BrandDetails>("OfferMS\\BrandDetail");

            _fareFamily.Setup(x => x.GetFareFamily(It.IsAny<string>())).ReturnsAsync(brandDetails.ItaFareFamily);
            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(4, result.Value.Value.DataList.BaggageAllowances.Count);
            Assert.True(result.Value.Value.Offers.First(x => x.Selected == true).OfferItems.All(x => x.Services.All(x => x.BaggageRefIds.Contains("BAG1"))));

        }


        [Fact]
        public async Task Handler_ShouldReturnSuccessResponse_ForSelectedIdRepriceResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponseForSingleSegmentSinglePtc");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");
            var brandDetails = TestDataProvider.GetTestData<BrandDetails>("OfferMS\\BrandDetail");

            _fareFamily.Setup(x => x.GetFareFamily(It.IsAny<string>())).ReturnsAsync(brandDetails.ItaFareFamily);
            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessResponse_ForNotSelectedIdRepriceResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponseForSingleSegmentSinglePtcOnlyUpsel");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");
            var brandDetails = TestDataProvider.GetTestData<BrandDetails>("OfferMS\\BrandDetail");

            _fareFamily.Setup(x => x.GetFareFamily(It.IsAny<string>())).ReturnsAsync(brandDetails.ItaFareFamily);
            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.RepriceNoSelectedIdCode));
        }

        [Fact]
        public async Task Handler_ShouldReturnOriginalSalePrice_InRepriceResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponseForSingleSegmentSinglePtc");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");
            var brandDetails = TestDataProvider.GetTestData<BrandDetails>("OfferMS\\BrandDetail");

            _fareFamily.Setup(x => x.GetFareFamily(It.IsAny<string>())).ReturnsAsync(brandDetails.ItaFareFamily);
            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(3, result.Value.Value.Offers.FirstOrDefault(x => x.Selected == true).OfferItems.FirstOrDefault().OfferitemPricing.FirstOrDefault().OriginalSalePrice.Count);
            Assert.Equal("122.84", result.Value.Value.Offers.FirstOrDefault(x => x.Selected == true).OfferItems.FirstOrDefault().OfferitemPricing.FirstOrDefault().OriginalSalePrice.FirstOrDefault(x => x.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount);
        }
        [Fact]
        public async Task Handler_ShouldGetResponse_ForLowerCasePTC()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            request.CustomerDetails.ForEach(x => x.Ptc.ToLower());
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponseForSingleSegmentSinglePtc");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");
            var brandDetails = TestDataProvider.GetTestData<BrandDetails>("OfferMS\\BrandDetail");

            _fareFamily.Setup(x => x.GetFareFamily(It.IsAny<string>())).ReturnsAsync(brandDetails.ItaFareFamily);
            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );
            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
        }
        [Fact]
        public async Task Handler_ShouldReturnNullOperatingCarrierandOperatingCarrierNumber_ForVSFlights()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            request.CustomerDetails.ForEach(x => x.Ptc.ToLower());
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceTripDetailsResponseForSingleSegmentSinglePtc");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");
            var brandDetails = TestDataProvider.GetTestData<BrandDetails>("OfferMS\\BrandDetail");

            _fareFamily.Setup(x => x.GetFareFamily(It.IsAny<string>())).ReturnsAsync(brandDetails.ItaFareFamily);
            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );
            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal("VS", result.Value.Value.DataList.FlightSegments.Last().MarketingCarrier);
            Assert.Null(result.Value.Value.DataList.FlightSegments.Last().OperatingCarrier);
            Assert.Null(result.Value.Value.DataList.FlightSegments.Last().OperatingFlightNumber);
        }
        [Fact]
        public async Task Handler_ShouldReturnOperatingCarrierandOperatingCarrierNumber_ForCodeShareFlights()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceResponseMultiSegmentSinglePTC_1ADT_DLFlights");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var amenitiesEntityResponse = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OfferMS\\AmenitiesTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");
            var brandDetails = TestDataProvider.GetTestData<BrandDetails>("OfferMS\\BrandDetail");

            _fareFamily.Setup(x => x.GetFareFamily(It.IsAny<string>())).ReturnsAsync(brandDetails.ItaFareFamily);
            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _cachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(amenitiesEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal("VS", result.Value.Value.DataList.FlightSegments.First().MarketingCarrier);
            Assert.Equal("DL", result.Value.Value.DataList.FlightSegments.First().OperatingCarrier);
            Assert.NotNull(result.Value.Value.DataList.FlightSegments.First().OperatingFlightNumber);
        }
        [Fact]
        public async Task Handler_ShouldReturnOperatingCarrierandOperatingCarrierNumber_ForNonVSFlights()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("OfferMS\\OrderRepriceValidRequest");
            var searchResult = TestDataProvider.GetXmlTestData<result>("OfferMS\\RepriceResponseITAServiceforNonVS");
            var cabinEntityResponse = TestDataProvider.GetTestData<CabinTableEntity>("OfferMS\\CabinTableEntityData");
            var tableEntityResponse = TestDataProvider.GetTestData<PriceClassTableEntity>("OfferMS\\PriceClassTableEntityData");
            var amenitiesEntityResponse = TestDataProvider.GetTestData<List<AmenitiesTableEntity>>("OfferMS\\AmenitiesTableEntityData");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS\\OrderSearchResponse");
            var brandDetails = TestDataProvider.GetTestData<BrandDetails>("OfferMS\\BrandDetail");

            _fareFamily.Setup(x => x.GetFareFamily(It.IsAny<string>())).ReturnsAsync(brandDetails.ItaFareFamily);
            _cabinDetail.Setup(r => r.GetCabinDetails()).Returns(
                TestDataProvider.GetTestData<List<Cabins>>("OfferMS\\CabinConfigData")
            );

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(cabinEntityResponse);
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(tableEntityResponse);
            _cachedAmenities.Setup(x => x.GetAmenitiesInformation(It.IsAny<string>())).ReturnsAsync(amenitiesEntityResponse);
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);
            _mediator.Setup(x => x.Send(It.IsAny<ShoppingDocumentRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act.
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal("AI", result.Value.Value.DataList.FlightSegments.First().MarketingCarrier);
            Assert.Equal("AI", result.Value.Value.DataList.FlightSegments.First().OperatingCarrier);
            Assert.NotNull(result.Value.Value.DataList.FlightSegments.First().OperatingFlightNumber);
        }
    }

}
