using Moq;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Application;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Tests
{
    public class HealthReportCommandHandlerTests
    {
        private readonly Mock<IHealthCheckAirShoppingService> _healthCheckAirShoppingService;
        private readonly Mock<IHealthCheckOfferPriceService> _healthCheckOfferPriceService;
        private readonly Mock<IHealthCheckOrderCreateService> _healthCheckOrderCreateService;
        private readonly Mock<IHealthCheckSeatAvailabilityService> _healthCheckSeatAvailabilityService;
        private readonly Mock<IHealthCheckOrderChangeService> _healthCheckOrderChangeService;
        private readonly Mock<IHealthCheckOrderHistoryService> _healthCheckOrderHistoryService;
        private readonly Mock<IHealthCheckOrderQuoteService> _healthCheckOrderQuoteService;
        private readonly Mock<IHealthCheckOrderReshopService> _healthCheckOrderReshopService;
        private readonly Mock<IHealthCheckOrderListService> _healthCheckOrderListService; 
        private readonly Mock<IHealthCheckOrderRetrieveService> _healthCheckOrderRetrieveService;
        private readonly Mock<IHealthCheckOCNService> _healthCheckOCNService;
        private readonly Mock<IHealthCheckServiceListService> _healthCheckServiceListService;

        private readonly IHealthCheckAirShoppingRepository _healthCheckAirShoppingRepository;
        private readonly IHealthCheckOfferPriceRepository _healthCheckOfferPriceRepository;
        private readonly IHealthCheckOrderCreateRepository _healthCheckOrderCreateRepository;
        private readonly IHealthCheckSeatAvailabilityRepository _healthCheckSeatAvailabilityRepository;
        private readonly IHealthCheckOrderChangeRepository _healthCheckOrderChangeRepository;
        private readonly IHealthCheckOrderHistoryRepository _healthCheckOrderHistoryRepository;
        private readonly IHealthCheckOrderQuoteRepository _healthCheckOrderQuoteRepository;
        private readonly IHealthCheckOrderReshopRepository _healthCheckOrderReshopRepository;
        private readonly IHealthCheckOrderListRepository _healthCheckOrderListRepository;
        private readonly IHealthCheckOrderRetrieveRepository _healthCheckOrderRetrieveRepository;
        private readonly IHealthCheckOCNRepository _healthCheckOCNRepository;
        private readonly IHealthCheckServiceListRepository _healthCheckServiceListRepository;

        private readonly HealthReportCommandHandler _handler;

        public HealthReportCommandHandlerTests()
        {
            _healthCheckAirShoppingService = new Mock<IHealthCheckAirShoppingService>();
            _healthCheckOfferPriceService = new Mock<IHealthCheckOfferPriceService>();
            _healthCheckOrderCreateService = new Mock<IHealthCheckOrderCreateService>();
            _healthCheckSeatAvailabilityService = new Mock<IHealthCheckSeatAvailabilityService>();
            _healthCheckOrderChangeService = new Mock<IHealthCheckOrderChangeService>();
            _healthCheckOrderHistoryService = new Mock<IHealthCheckOrderHistoryService>();
            _healthCheckOrderQuoteService = new Mock<IHealthCheckOrderQuoteService>();
            _healthCheckOrderReshopService = new Mock<IHealthCheckOrderReshopService>();
            _healthCheckOrderListService = new Mock<IHealthCheckOrderListService>();
            _healthCheckOrderRetrieveService = new Mock<IHealthCheckOrderRetrieveService>();
            _healthCheckOCNService = new Mock<IHealthCheckOCNService>();
            _healthCheckServiceListService = new Mock<IHealthCheckServiceListService>();

            _healthCheckAirShoppingRepository = new HealthCheckAirShoppingRepository(_healthCheckAirShoppingService.Object);
            _healthCheckOfferPriceRepository = new HealthCheckOfferPriceRepository(_healthCheckOfferPriceService.Object);
            _healthCheckOrderCreateRepository = new HealthCheckOrderCreateRepository(_healthCheckOrderCreateService.Object);
            _healthCheckSeatAvailabilityRepository = new HealthCheckSeatAvailabilityRepository(_healthCheckSeatAvailabilityService.Object);
            _healthCheckOrderChangeRepository = new HealthCheckOrderChangeRepository(_healthCheckOrderChangeService.Object);
            _healthCheckOrderHistoryRepository = new HealthCheckOrderHistoryRepository(_healthCheckOrderHistoryService.Object);
            _healthCheckOrderQuoteRepository = new HealthCheckOrderQuoteRepository(_healthCheckOrderQuoteService.Object);
            _healthCheckOrderReshopRepository = new HealthCheckOrderReshopRepository(_healthCheckOrderReshopService.Object);
            _healthCheckOrderListRepository = new HealthCheckOrderListRepository(_healthCheckOrderListService.Object);
            _healthCheckOrderRetrieveRepository = new HealthCheckOrderRetrieveRepository(_healthCheckOrderRetrieveService.Object);
            _healthCheckOCNRepository = new HealthCheckOCNRepository(_healthCheckOCNService.Object);
            _healthCheckServiceListRepository = new HealthCheckServiceListRepository(_healthCheckServiceListService.Object);

            _handler = new HealthReportCommandHandler(_healthCheckAirShoppingRepository, _healthCheckOfferPriceRepository, _healthCheckOrderCreateRepository, _healthCheckSeatAvailabilityRepository, _healthCheckOrderChangeRepository, _healthCheckOrderHistoryRepository, _healthCheckOrderQuoteRepository, _healthCheckOrderReshopRepository, _healthCheckOrderListRepository, _healthCheckOrderRetrieveRepository, _healthCheckOCNRepository, _healthCheckServiceListRepository);

        }

        [Fact]
        public async Task Handle_Should_Return_HealthReport()
        {
            // Arrange
            _healthCheckAirShoppingService.Setup(service => service.GetAirShoppingHealthReport()).ReturnsAsync(
            [
                 new(){ StatusCode = "200", Percentage = 100, RequestCount = 100, CalendarSearch = "N" },
                 new(){ StatusCode = "200", Percentage = 95, RequestCount = 95, CalendarSearch = "Y" },
                 new(){ StatusCode = "500", Percentage = 5, RequestCount = 5, CalendarSearch = "Y" }
            ]);

            _healthCheckOfferPriceService.Setup(service => service.GetOfferPriceHealthReport()).ReturnsAsync(
            [
                new(){ StatusCode = "200" , Percentage = 90 , RequestCount = 90},
                new(){ StatusCode = "500" , Percentage = 10 , RequestCount = 10}
            ]);

            _healthCheckSeatAvailabilityService.Setup(service => service.GetSeatAvailabilityHealthReport()).ReturnsAsync(
            [
                new(){ StatusCode = "200", Percentage = 90, RequestCount = 90, IsPrime = "Y" },
                new(){ StatusCode = "500", Percentage = 10, RequestCount = 10, IsPrime = "N" }
            ]);

            _healthCheckOrderCreateService.Setup(service => service.GetOrderCreateHealthReport()).ReturnsAsync(
            [
                new(){ StatusCode = "200" , Percentage = 90 , RequestCount = 90, HoldBooking = "Y" , InstantBooking = "N" , Seat = "N"},
                new(){ StatusCode = "500" , Percentage = 10 , RequestCount = 10, HoldBooking = "Y" , InstantBooking = "N" , Seat = "N"},
                new(){ StatusCode = "200" , Percentage = 80 , RequestCount = 80, HoldBooking = "N" , InstantBooking = "Y" , Seat = "N"},
                new(){ StatusCode = "500" , Percentage = 20 , RequestCount = 20, HoldBooking = "N" , InstantBooking = "Y" , Seat = "N"},
                new(){ StatusCode = "200" , Percentage = 100 , RequestCount = 100, HoldBooking = "N" , InstantBooking = "Y" , Seat = "Y"},
            ]);

            _healthCheckOrderHistoryService.Setup(service => service.GetOrderHistoryHealthReport()).ReturnsAsync(
           [
               new(){ StatusCode = "200" , Percentage = 90 , RequestCount = 90},
                new(){ StatusCode = "500" , Percentage = 10 , RequestCount = 10}
           ]);

            _healthCheckOrderQuoteService.Setup(service => service.GetOrderQuoteHealthReport()).ReturnsAsync(
           [
               new(){ StatusCode = "200" , Percentage = 90 , RequestCount = 90},
                new(){ StatusCode = "500" , Percentage = 10 , RequestCount = 10}
           ]);

            _healthCheckOrderReshopService.Setup(service => service.GetOrderReshopHealthReport()).ReturnsAsync(
            [
                new(){ StatusCode = "200", Percentage = 90, RequestCount = 90, ReshopRequest = "Y", CancelReshopRequest = "N", NameCorrectionReshopRequest = "N" },
                new(){ StatusCode = "500", Percentage = 10, RequestCount = 10, ReshopRequest = "N", CancelReshopRequest = "Y", NameCorrectionReshopRequest = "N" },
                new(){ StatusCode = "200", Percentage = 80, RequestCount = 80, ReshopRequest = "N", CancelReshopRequest = "N", NameCorrectionReshopRequest = "Y" }
            ]);

            _healthCheckOrderListService.Setup(service => service.GetOrderListHealthReport()).ReturnsAsync(
           [
               new(){ StatusCode = "200" , Percentage = 90 , RequestCount = 90},
                new(){ StatusCode = "500" , Percentage = 10 , RequestCount = 10}
           ]);

            _healthCheckOrderRetrieveService.Setup(service => service.GetOrderRetrieveHealthReport()).ReturnsAsync(
           [
                new(){ StatusCode = "200" , Percentage = 90 , RequestCount = 90},
                new(){ StatusCode = "500" , Percentage = 10 , RequestCount = 10}
           ]);

            _healthCheckOCNService.Setup(service => service.GetOCNHealthReport()).ReturnsAsync(
           [
                new(){ StatusCode = "200" , Percentage = 90 , RequestCount = 90},
                new(){ StatusCode = "500" , Percentage = 10 , RequestCount = 10}
           ]);

            _healthCheckServiceListService.Setup(service => service.GetServiceListHealthReport()).ReturnsAsync(
            [
                new(){ StatusCode = "200", Percentage = 90, RequestCount = 90, IsPrime = "Y" },
                new(){ StatusCode = "500", Percentage = 10, RequestCount = 10, IsPrime = "N" }
            ]);

            // Act
            var result = await _handler.FetchHealthReport(["AirShopping", "OfferPrice", "OrderCreate", "SeatAvailability", "OrderHistory", "OrderQuote", "OrderReshop", "OrderList", "OrderRetrieve", "OrderChangeNotif" , "ServiceList"]);


            // Assert
            Assert.NotNull(result);
            Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.OK, result.Status);

            var airShoppingReport = result.Value.HealthReport.First(x => x.API == "AirShopping");
            Assert.Equal(1, airShoppingReport.StatusInfo.Count()); 
            Assert.Equal(100, airShoppingReport.StatusInfo.First(x => x.StatusCode == "200").Percentage);

            var calendarSearchReport = result.Value.HealthReport.First(x => x.API == "AirShoppingCalendarSearch");
            Assert.Equal(2, calendarSearchReport.StatusInfo.Count()); 
            Assert.Equal(95, calendarSearchReport.StatusInfo.First(x => x.StatusCode == "200").Percentage);

            var offerPriceReport = result.Value.HealthReport.First(x => x.API == "OfferPrice");
            Assert.Equal(2, offerPriceReport.StatusInfo.Count());
            Assert.Equal(90, offerPriceReport.StatusInfo.First(x => x.StatusCode == "200").Percentage);

            var primeReport = result.Value.HealthReport.First(x => x.API == "PrimeSaleSeatAvailability");
            Assert.Single(primeReport.StatusInfo);

            var postReport = result.Value.HealthReport.First(x => x.API == "PostSaleSeatAvailability");
            Assert.Single(postReport.StatusInfo);

            var orderCreateHBReport = result.Value.HealthReport.First(x => x.API == "OrderCreateHoldBooking");
            Assert.Equal(2, orderCreateHBReport.StatusInfo.Count());
            Assert.Equal(90, orderCreateHBReport.StatusInfo.First(x => x.StatusCode == "200").Percentage);

            var orderCreateIPReport = result.Value.HealthReport.First(x => x.API == "OrderCreateInstantPurchase");
            Assert.Equal(2, orderCreateIPReport.StatusInfo.Count());
            Assert.Equal(80, orderCreateIPReport.StatusInfo.First(x => x.StatusCode == "200").Percentage);

            var orderCreateSeatReport = result.Value.HealthReport.First(x => x.API == "OrderCreateInstantPurchaseSeat");
            Assert.Single(orderCreateSeatReport.StatusInfo);
            Assert.Equal(100, orderCreateSeatReport.StatusInfo.First(x => x.StatusCode == "200").Percentage);

            var orderHistoryReport = result.Value.HealthReport.First(x => x.API == "OrderHistory");
            Assert.Equal(2, orderHistoryReport.StatusInfo.Count());
            Assert.Equal(90, orderHistoryReport.StatusInfo.First(x => x.StatusCode == "200").Percentage);

            var orderQuoteReport = result.Value.HealthReport.First(x => x.API == "OrderQuote");
            Assert.Equal(2, orderQuoteReport.StatusInfo.Count());

            var reshopReport = result.Value.HealthReport.First(x => x.API == "OrderReshop");
            Assert.Single(reshopReport.StatusInfo); 

            var cancelReport = result.Value.HealthReport.First(x => x.API == "OrderReshopCancel");
            Assert.Single(cancelReport.StatusInfo); 

            var nameCorrectionReport = result.Value.HealthReport.First(x => x.API == "OrderReshopNameCorrection");
            Assert.Single(nameCorrectionReport.StatusInfo); 

            var orderListReport = result.Value.HealthReport.First(x => x.API == "OrderList");
            Assert.Equal(2, orderListReport.StatusInfo.Count());

            var orderRetrieveReport = result.Value.HealthReport.First(x => x.API == "OrderRetrieve");
            Assert.Equal(2, orderRetrieveReport.StatusInfo.Count());
            Assert.Equal(90, orderRetrieveReport.StatusInfo.First(x => x.StatusCode == "200").Percentage);

            var ocnReport = result.Value.HealthReport.First(x => x.API == "OrderChangeNotif");
            Assert.Equal(2, ocnReport.StatusInfo.Count());

            var primeServiceListReport = result.Value.HealthReport.First(x => x.API == "PrimeSaleServiceList");
            Assert.Single(primeReport.StatusInfo);

            var postServiceListReport = result.Value.HealthReport.First(x => x.API == "PostSaleServiceList");
            Assert.Single(postReport.StatusInfo);
        }
    }
}