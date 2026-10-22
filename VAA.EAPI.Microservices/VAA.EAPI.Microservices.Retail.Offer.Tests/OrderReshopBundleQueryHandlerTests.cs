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
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class OrderReshopBundleQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderReshopBundleQueryHandler>> _logger;
        private readonly Mock<ICachedCabin> _cabinDetail;
        private readonly Mock<ICachedPriceClass> _cachedPriceClass;
        private readonly OrderReshopBundleQueryHandler _handler;
        private readonly Mock<ICachedCabinBaggage> _cachedCabinBaggage;
        private readonly Mock<ICachedPieceAllowance> _cachedPieceAllowance;
        private readonly Mock<ICachedAirportDetails> _cachedAirportDetails;
        private readonly Mock<IDistributedCacheService> _distributedCacheService;

        public OrderReshopBundleQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderReshopBundleQueryHandler>>();
            _cabinDetail = new Mock<ICachedCabin>();
            _cachedPriceClass = new Mock<ICachedPriceClass>();
            _cachedCabinBaggage = new Mock<ICachedCabinBaggage>();
            _cachedPieceAllowance = new Mock<ICachedPieceAllowance>();
            _cachedAirportDetails = new Mock<ICachedAirportDetails>();
            _distributedCacheService = new Mock<IDistributedCacheService>();

            _handler = new OrderReshopBundleQueryHandler(_mediator.Object, _logger.Object, _cachedPriceClass.Object, _cabinDetail.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _cachedAirportDetails.Object, _distributedCacheService.Object);
            
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
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderReshopBundleQueryHandler(null, _logger.Object, _cachedPriceClass.Object, _cabinDetail.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _cachedAirportDetails.Object, _distributedCacheService.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderReshopBundleQueryHandler(_mediator.Object, null, _cachedPriceClass.Object, _cabinDetail.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _cachedAirportDetails.Object, _distributedCacheService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCabinObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderReshopBundleQueryHandler(_mediator.Object, _logger.Object, _cachedPriceClass.Object, null, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _cachedAirportDetails.Object, _distributedCacheService.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenPriceClassObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderReshopBundleQueryHandler(_mediator.Object, _logger.Object, null, _cabinDetail.Object, _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _cachedAirportDetails.Object, _distributedCacheService.Object));
        }
        [Fact]
        public async Task Handler_SuccessResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _cabinDetail.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Business"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });

            _cachedAirportDetails.Setup(x => x.GetCachedAirportDetails(It.IsAny<string>())).ReturnsAsync(new AirportDetailsTableEntity()
            {
                RowKey = "LHR",
                CityCode = "LON"
            });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
        }
        [Fact]
        public async Task Handler_SuccessResponse_SplitDocument()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _cabinDetail.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Business"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });

            _cachedAirportDetails.Setup(x => x.GetCachedAirportDetails(It.IsAny<string>())).ReturnsAsync(new AirportDetailsTableEntity()
            {
                RowKey = "LHR",
                CityCode = "LON"
            });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
        }
        [Fact]
        public async Task Handler_FailureResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledInValidRequest");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);
            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).Throws<Exception>();
            //Act
            var result = await _handler.Handle(request, CancellationToken.None);
            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.AddFieldMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.AddFieldMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.PriceComponentTypeIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.PriceComponentTypeIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.PriceComponentNameIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.PriceComponentNameIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.AmountIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.AmountIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.MonetaryAmountIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.MonetaryAmountIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.BaseCurrencyIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.BaseCurrencyIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.DocumentTypeIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.DocumentTypeIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.DocumentNumberIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.DocumentNumberIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.IssueDateIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.IssueDateIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.CouponStatusMissing));
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OdLabelIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OdLabelIsMissing).FirstOrDefault()?.Message);
        }
        [Fact]
        public async Task Handler_ShouldReturnExpirationTimeLimit_ForGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _cabinDetail.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Bussiness"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });

            _cachedAirportDetails.Setup(x => x.GetCachedAirportDetails(It.IsAny<string>())).ReturnsAsync(new AirportDetailsTableEntity()
            {
                RowKey = "LHR",
                CityCode = "LON"
            });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value.Value.Offers.First().OfferExpirationTimeLimit);
        }
        [Fact]
        public async Task Handler_ShouldReturnNullBaggage_ForFlownReshopRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\C5AEAY_FLOWN_ReshopRequest");
            request.ReshopQueries.FlightSpecific.FlightsRequest.Retain.ForEach(x => x.SegmentId = new List<string> { "02" });
            request.ReshopQueries.FlightSpecific.FlightsRequest.Add.ForEach(x => x.SegmentId = new List<string> { "02" });
            request.OriginDestinationAssociations.ForEach(a => a.OriginDestination.DepartureDate = "2022-12-25");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse_FLOWN.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _cabinDetail.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Bussiness"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });

            _cachedAirportDetails.Setup(x => x.GetCachedAirportDetails(It.IsAny<string>())).ReturnsAsync(new AirportDetailsTableEntity()
            {
                RowKey = "LHR",
                CityCode = "LON"
            });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert

            Assert.Null(result.Value.Value.Offers.FirstOrDefault().OfferItems.FirstOrDefault().Services.FirstOrDefault().BaggageRefIds);
            Assert.Equal("JOUR1", result.Value.Value.Offers.FirstOrDefault().OfferItems.FirstOrDefault().Services.FirstOrDefault().JourneyRefIds.FirstOrDefault());

        }
        [Fact]
        public async Task Handler_ShouldThrowError_ForInvalidPTC()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            request.CustomerDetails.ForEach(x => x.Ptc = "TestPTC");
            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.PtcIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.PtcIsMissing).FirstOrDefault()?.Message);
        }
        [Fact]
        public async Task Handler_ShouldSucceed_ForLowerCasePTC()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            request.CustomerDetails.ForEach(x => x.Ptc.ToLower());
            request.OriginDestinationAssociations.ForEach(a => a.OriginDestination.DepartureDate = "2022-12-25");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _cabinDetail.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Bussiness"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });

            _cachedAirportDetails.Setup(x => x.GetCachedAirportDetails(It.IsAny<string>())).ReturnsAsync(new AirportDetailsTableEntity()
            {
                RowKey = "LHR",
                CityCode = "LON"
            });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
        }
        [Fact]
        public async Task Handler_ShouldThrowError_ForNullServiceBeneficiaries_SegmentID_OnDIDinAddRetainDelete()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            request.ReshopOrder.ForEach(x => x.Services.ForEach(x => x.ServiceBeneficiaries = new List<string> { null }));
            request.SegmentDetails.ForEach(x => x.SegmentId = null);
            request.ReshopQueries.FlightSpecific.FlightsRequest.Retain.ForEach(x => x.SegmentId = null);
            request.ReshopQueries.FlightSpecific.FlightsRequest.Add.ForEach(x => x.OriginDestinationId = "TestID");
            request.ReshopQueries.FlightSpecific.FlightsRequest.Retain.ForEach(x => x.OriginDestinationId = null);
            request.ReshopQueries.FlightSpecific.FlightsRequest.Delete.ForEach(x => x.OriginDestinationId = null);
            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.ServiceBeneficiaryIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.ServiceBeneficiaryIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.SegmentIDIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.SegmentIDIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.SegmentIDMissingorInvalid));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.SegmentIDMissingorInvalid).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidRetain));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidRetain).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidAdd));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidAdd).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidDelete));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidDelete).FirstOrDefault()?.Message);

        }
        [Fact]
        public async Task Handler_ShouldThrowError_ForEmptyServiceBeneficiaries_SegmentID_OnDIDinAddRetainDelete()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            request.ReshopOrder.ForEach(x => x.Services.ForEach(x => x.ServiceBeneficiaries = new List<string> { "" }));
            request.SegmentDetails.ForEach(x => x.SegmentId = "");
            request.ReshopQueries.FlightSpecific.FlightsRequest.Retain.ForEach(x => x.SegmentId = new List<string> { "" });
            request.ReshopQueries.FlightSpecific.FlightsRequest.Delete.ForEach(x => x.OriginDestinationId = "");
            request.ReshopQueries.FlightSpecific.FlightsRequest.Add.ForEach(x => x.OriginDestinationId = "TestID");
            request.ReshopQueries.FlightSpecific.FlightsRequest.Retain.ForEach(x => x.OriginDestinationId = "");
            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.ServiceBeneficiaryIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.ServiceBeneficiaryIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.SegmentIDIsMissing));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.SegmentIDIsMissing).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.SegmentIDMissingorInvalid));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.SegmentIDMissingorInvalid).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidRetain));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidRetain).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidAdd));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidAdd).FirstOrDefault()?.Message);
            Assert.NotEmpty(result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidDelete));
            Assert.Contains("One of the mandatory field is missing or invalid in the request", result.Value.Errors.Where(item => item.Code == Constants.ErrorCodes.OnDIDInvalidDelete).FirstOrDefault()?.Message);
        }

        [Fact]
        public async Task Handler_ShouldReturnOriginalSalePrice_ForGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _cabinDetail.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Bussiness"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });

            _cachedAirportDetails.Setup(x => x.GetCachedAirportDetails(It.IsAny<string>())).ReturnsAsync(new AirportDetailsTableEntity()
            {
                RowKey = "LHR",
                CityCode = "LON"
            });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(3, result.Value.Value.Offers.FirstOrDefault().OfferItems.FirstOrDefault().OfferitemPricing.FirstOrDefault().OriginalSalePrice.Count);
            Assert.Equal("320.52", result.Value.Value.Offers.FirstOrDefault().OfferItems.FirstOrDefault().OfferitemPricing.FirstOrDefault().OriginalSalePrice.FirstOrDefault(x => x.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount);
        }
        [Fact]
        public async Task Handler_ShouldReturnNullOperatingCarrierandOperatingCarrierNumber_ForVSFlights()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _cabinDetail.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Bussiness"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });

            _cachedAirportDetails.Setup(x => x.GetCachedAirportDetails(It.IsAny<string>())).ReturnsAsync(new AirportDetailsTableEntity()
            {
                RowKey = "LHR",
                CityCode = "LON"
            });

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
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _cabinDetail.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Bussiness"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });

            _cachedAirportDetails.Setup(x => x.GetCachedAirportDetails(It.IsAny<string>())).ReturnsAsync(new AirportDetailsTableEntity()
            {
                RowKey = "LHR",
                CityCode = "LON"
            });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal("VS", result.Value.Value.DataList.FlightSegments.First().MarketingCarrier);
            Assert.Equal("KL", result.Value.Value.DataList.FlightSegments.First().OperatingCarrier);
            Assert.NotNull(result.Value.Value.DataList.FlightSegments.First().OperatingFlightNumber);
        }
        [Fact]
        public async Task Handler_ShouldReturnOperatingCarrierandOperatingCarrierNumber_ForNonVSFlights()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderReshopBundleQuery>("OfferMS\\OrderReshopBundledValidRequest");
            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponseNonVS.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _mediator.Setup(x => x.Send(It.IsAny<ItaRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResult);

            _cabinDetail.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Bussiness"
            });

            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });

            _cachedAirportDetails.Setup(x => x.GetCachedAirportDetails(It.IsAny<string>())).ReturnsAsync(new AirportDetailsTableEntity()
            {
                RowKey = "LHR",
                CityCode = "LON"
            });

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal("AI", result.Value.Value.DataList.FlightSegments.First().MarketingCarrier);
            Assert.Equal("AI", result.Value.Value.DataList.FlightSegments.First().OperatingCarrier);
            Assert.NotNull(result.Value.Value.DataList.FlightSegments.First().OperatingFlightNumber);
        }
    }
}
