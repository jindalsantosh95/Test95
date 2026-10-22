using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Application
{
    public class HealthReportCommandHandler : IHealthReportCommand
    {
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

        public HealthReportCommandHandler(IHealthCheckAirShoppingRepository healthCheckAirShoppingRepository, IHealthCheckOfferPriceRepository healthCheckOfferPriceRepository, IHealthCheckOrderCreateRepository healthCheckOrderCreateRepository, IHealthCheckSeatAvailabilityRepository healthCheckSeatAvailabilityRepository, IHealthCheckOrderChangeRepository healthCheckOrderChangeRepository, IHealthCheckOrderHistoryRepository healthCheckOrderHistoryRepository, IHealthCheckOrderQuoteRepository healthCheckOrderQuoteRepository, IHealthCheckOrderReshopRepository healthCheckOrderReshopRepository, IHealthCheckOrderListRepository healthCheckOrderListRepository, IHealthCheckOrderRetrieveRepository healthCheckOrderRetrieveRepository, IHealthCheckOCNRepository healthCheckOCNRepository, IHealthCheckServiceListRepository healthCheckServiceListRepository)
        {
            _healthCheckAirShoppingRepository = healthCheckAirShoppingRepository;
            _healthCheckOfferPriceRepository = healthCheckOfferPriceRepository;
            _healthCheckOrderCreateRepository = healthCheckOrderCreateRepository;
            _healthCheckSeatAvailabilityRepository = healthCheckSeatAvailabilityRepository;
            _healthCheckOrderChangeRepository =  healthCheckOrderChangeRepository;
            _healthCheckOrderHistoryRepository = healthCheckOrderHistoryRepository;
            _healthCheckOrderQuoteRepository = healthCheckOrderQuoteRepository;
            _healthCheckOrderReshopRepository = healthCheckOrderReshopRepository;
            _healthCheckOrderListRepository = healthCheckOrderListRepository;
            _healthCheckOrderRetrieveRepository = healthCheckOrderRetrieveRepository;
            _healthCheckOCNRepository = healthCheckOCNRepository;
            _healthCheckServiceListRepository = healthCheckServiceListRepository;
        }

        public async Task<Response<HealthReportResponse>> FetchHealthReport(IEnumerable<string> apiList)
        {
            var response = new List<HealthReport>();

            if(apiList.Any(a => a.Is("AirShopping")))
            {
                var airShoppingHealthReport = await _healthCheckAirShoppingRepository.GetAirShoppingHealthReport();
                response.Add(new HealthReport()
                {
                    API = "AirShopping",
                    StatusInfo = airShoppingHealthReport.Where(x => x.CalendarSearch == "N").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "AirShoppingCalendarSearch",
                    StatusInfo = airShoppingHealthReport.Where(x => x.CalendarSearch == "Y").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if (apiList.Any(a => a.Is("OfferPrice")))
            {
                var offerPriceHealthReport = await _healthCheckOfferPriceRepository.GetOfferPriceHealthReport();
                response.Add(new HealthReport()
                {
                    API = "OfferPrice",
                    StatusInfo = offerPriceHealthReport.Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if(apiList.Any(a => a.Is("OrderCreate")))
            {
                var orderCreateHealthReport = await _healthCheckOrderCreateRepository.GetOrderCreateHealthReport();
                response.Add(new HealthReport()
                {
                    API = "OrderCreateInstantPurchase",
                    StatusInfo = orderCreateHealthReport.Where(x => x.InstantBooking == "Y" && x.HoldBooking == "N" && x.Seat == "N").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderCreateHoldBooking",
                    StatusInfo = orderCreateHealthReport.Where(x => x.HoldBooking == "Y").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderCreateInstantPurchaseSeat",
                    StatusInfo = orderCreateHealthReport.Where(x => x.InstantBooking == "Y" && x.Seat == "Y").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if (apiList.Any(x => x.Contains("SeatAvailability")))
            {
                var seatAvailabilityHealthReport = await _healthCheckSeatAvailabilityRepository.GetSeatAvailabilityHealthReport();
                response.Add(new HealthReport()
                {
                    API = "PrimeSaleSeatAvailability",
                    StatusInfo = seatAvailabilityHealthReport.Where(x => x.IsPrime == "Y").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "PostSaleSeatAvailability",
                    StatusInfo = seatAvailabilityHealthReport.Where(x => x.IsPrime == "N").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }
            
            if (apiList.Any(a => a.Is("OrderChange")))
            {
                var orderChangeHealthReport = await _healthCheckOrderChangeRepository.GetOrderChangeHealthReport();
                response.Add(new HealthReport()
                {
                    API = "OrderChange_AddCollect",
                    StatusInfo = orderChangeHealthReport.Where(x => x.RequestType == "OrderChange_AddCollect").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderChange_AmendApis",
                    StatusInfo = orderChangeHealthReport.Where(x => x.RequestType == "OrderChange_AmendApis").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderChange_Cancel",
                    StatusInfo = orderChangeHealthReport.Where(x => x.RequestType == "OrderChange_Cancel").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderChange_EvenExchange",
                    StatusInfo = orderChangeHealthReport.Where(x => x.RequestType == "OrderChange_EvenExchange").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
                
                response.Add(new HealthReport()
                {
                    API = "OrderChange_HoldBooking",
                    StatusInfo = orderChangeHealthReport.Where(x => x.RequestType == "OrderChange_HoldBooking").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderChange_Refund",
                    StatusInfo = orderChangeHealthReport.Where(x => x.RequestType == "OrderChange_Refund").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderChange_SplitPNR",
                    StatusInfo = orderChangeHealthReport.Where(x => x.RequestType == "OrderChange_SplitPNR").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderChange_PostSaleSeat",
                    StatusInfo = orderChangeHealthReport.Where(x => x.RequestType == "OrderChange_PostSaleSeat").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if (apiList.Any(a => a.Is("OrderHistory")))
            {
                var orderHistoryHealthReport = await _healthCheckOrderHistoryRepository.GetOrderHistoryHealthReport();
                response.Add(new HealthReport()
                {
                    API = "OrderHistory",
                    StatusInfo = orderHistoryHealthReport.Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if (apiList.Any(a => a.Is("OrderQuote")))
            {
                var orderQuoteHealthReport = await _healthCheckOrderQuoteRepository.GetOrderQuoteHealthReport();
                response.Add(new HealthReport()
                {
                    API = "OrderQuote",
                    StatusInfo = orderQuoteHealthReport.Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if (apiList.Any(a => a.Is("OrderReshop")))
            {
                var orderReshopHealthReport = await _healthCheckOrderReshopRepository.GetOrderReshopHealthReport();
                response.Add(new HealthReport()
                {
                    API = "OrderReshop",
                    StatusInfo = orderReshopHealthReport.Where(x => x.ReshopRequest == "Y").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderReshopCancel",
                    StatusInfo = orderReshopHealthReport.Where(x => x.CancelReshopRequest == "Y").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "OrderReshopNameCorrection",
                    StatusInfo = orderReshopHealthReport.Where(x => x.NameCorrectionReshopRequest == "Y").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if (apiList.Any(a => a.Is("OrderList")))
            {
                var orderListHealthReport = await _healthCheckOrderListRepository.GetOrderListHealthReport();
                response.Add(new HealthReport()
                {
                    API = "OrderList",
                    StatusInfo = orderListHealthReport.Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if (apiList.Any(a => a.Is("OrderRetrieve")))
            {
                var orderRetrieveHealthReport = await _healthCheckOrderRetrieveRepository.GetOrderRetrieveHealthReport();
                response.Add(new HealthReport()
                {
                    API = "OrderRetrieve",
                    StatusInfo = orderRetrieveHealthReport.Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if (apiList.Any(a => a.Is("OrderChangeNotif")))
            {
                var ocnHealthReport = await _healthCheckOCNRepository.GetOCNHealthReport();
                response.Add(new HealthReport()
                {
                    API = "OrderChangeNotif",
                    StatusInfo = ocnHealthReport.Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }

            if (apiList.Any(a => a.Is("ServiceList")))
            {
                var serviceListHealthReport = await _healthCheckServiceListRepository.GetServiceListHealthReport();
                
                response.Add(new HealthReport()
                {
                    API = "PrimeSaleServiceList",
                    StatusInfo = serviceListHealthReport.Where(x => x.IsPrime == "Y").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });

                response.Add(new HealthReport()
                {
                    API = "PostSaleServiceList",
                    StatusInfo = serviceListHealthReport.Where(x => x.IsPrime == "N").Select(x => new StatusInfo()
                    {
                        Count = x.RequestCount,
                        Percentage = x.Percentage,
                        StatusCode = x.StatusCode
                    })
                });
            }
           
            return new Response<HealthReportResponse>()
            {
                Status = Status.OK,
                Value = new HealthReportResponse() { HealthReport = response }
            };
        }
    }
}
