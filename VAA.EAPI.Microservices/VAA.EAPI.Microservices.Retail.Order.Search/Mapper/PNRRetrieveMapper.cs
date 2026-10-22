using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper
{
    public sealed class PNRRetrieveMapper
    {
        public static RetrieveRequestType RetrieveRequestMapper(OrderSearchRequest request)
        {
            return new RetrieveRequestType()
            {
                SetupContextRequest = new SetupContextRequestType()
                {
                    RequestInfo = new RequestInfoType()
                    {
                        ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                        DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                        TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                        HostContextParams = new HostContextParamsType()
                        {
                            PoolType = ConfigReader.Get(Constants.Environment.PoolType)
                        }
                    },
                    RetrieveInfo = new RetrieveInfoType()
                    {
                        PassengerInfo = new PassengerInfoType()
                        {
                            RecordLocator = request.SearchFilterCriteria.bookingReferenceCriteria.BookingReference.RLOC
                        }
                    },
                    StampingInfo = new StampingInfoType()
                    {
                        UserAuthorizationData = new UserAuthorizationDataType()
                        {
                            AgentCityCode = ConfigReader.Get(Constants.Environment.AgentCityCode),
                            RoleIndicator = ConfigReader.Get(Constants.Environment.RoleIndicator),
                            SecurityId = ConfigReader.Get(Constants.Environment.SecurityId)
                        }
                    }
                }
            };
        }
        public static async Task<OrderSearchResponse> RetrieveResponseMapper(RetrieveResponseType response, ICachedAirport cachedAirport, EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType ticketingResponse, List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType> ticketingResponseList, IProductName productName, ICachedSegmentStatusCode cachedSegmentStatusCode, ICachedCabinType cachedCabinType, ICachedAmenities cachedAmenities, IMediator mediator, List<EAPI.Domain.Virgin.Models.Customer.Customer> customers)
        {
            string orderId = Guid.NewGuid().ToString();
            bool isGDSbooking = response.TeardownContextResponse.Pnr.RecordLocator.First().OutsideSystemRecordLocator != null;

            Dictionary<string, string> specialServiceRequests = new();
            List<OriginDestinationAssociations> originDestinationAssociations = new();

            List<OrderSearchSegmentInfo> orderSearchSegmentInfos = GetOrderSearchSegmentInfos(response, isGDSbooking , ticketingResponse);
            Dictionary<string, string> segmentODMapper = OriginDestinationAssociationsMapper.Air4ToOrderSearchResponseMapper(response, ticketingResponse, originDestinationAssociations, orderSearchSegmentInfos);
            
            foreach(var od in originDestinationAssociations)
            {
                if (od.OriginDestination.Duration != null || !string.IsNullOrEmpty(od.OriginDestination.Duration))
                    od.OriginDestination.Duration = GetDurationFormat(od.OriginDestination.Duration, ConfigReader.Get(Constants.Environment.DurationFormat));
            }

            var passengers = response.TeardownContextResponse.Pnr.Passenger.ToList();

            if (response.TeardownContextResponse.Pnr.InfantInArms != null)
            {
                foreach (var passenger in response.TeardownContextResponse.Pnr.InfantInArms)
                {
                    passengers.Add(new Passenger()
                    {
                        CustomerId = passenger.CustomerId,
                        FirstName = passenger.FirstName,
                        FirstNameNumber = passenger.FirstNameNumber,
                        LastName = passenger.LastName.Split('/')[1],
                        Title = passenger.Title,
                        RecordName = passenger.RecordName,
                        NonRevenueStandbyFlag = passenger.NonRevenueStandbyFlag,
                        SegmentNumber = passenger.SegmentNumber,
                        Suffix = passenger.Suffix,
                        RevenueStandbyFlag = passenger.RevenueStandbyFlag,
                        LastNameNumber = passenger.LastNameNumber
                    });
                }
            }
            List<SegmentDetails> segmentDetails = await SegmentDetailsMapper.Air4ToOrderSearchResponseMapper(response, passengers, cachedAirport, originDestinationAssociations, segmentODMapper, cachedSegmentStatusCode, cachedCabinType, orderSearchSegmentInfos);
            
            foreach(var seg in segmentDetails)
            {
                if (!string.IsNullOrEmpty(seg.FlightDuration))
                    seg.FlightDuration = GetDurationFormat(seg.FlightDuration, ConfigReader.Get(Constants.Environment.DurationFormat));
            }
            
            List<OrderInstructions> orderInstructions = OrderInstructionsMapper.Air4ToOrderInstructionMapper(response);

            List<OrderItems> orderItems = OrderItemsMapper.Air4ToOrderSearchResponseMapper(response, passengers, orderId, orderInstructions, productName, orderSearchSegmentInfos, ticketingResponseList, isGDSbooking);

            return new OrderSearchResponse()
            {
                OriginDestinationAssociations = originDestinationAssociations,
                CustomerDetails = CustomerDetailsMapper.Air4ToOrderSearchResponseMapper(response, passengers, customers),
                OrderInstructions = orderInstructions,
                BookingReferences = new BookingReferences()
                {
                    AirlineDesignator = Constants.Application.VSFlightCode,
                    RLOC = response.TeardownContextResponse.Pnr.RecordLocator?.FirstOrDefault().RecordLocator1,
                    RLOCCreateDate = response.TeardownContextResponse.Pnr.RecordLocator?.FirstOrDefault().PnrCreationDateWithYear,
                    ExternalRecordLocator = string.Empty
                },
                CreationDateTime = new CreationDateTime()
                {
                    LocalDateTime = DateTimeHelper.Air4PNRDateTimeConversion(response.TeardownContextResponse.Pnr?.RecordLocator?.FirstOrDefault().PnrCreationDateWithYear, Constants.Application.DefaultDeltaTime)?.ToString(Constants.Application.ISODateTimeFormat),
                    UtcDateTime = string.Empty
                },
                OrderItems = orderItems,
                OrderNumber = new OrderNumber()
                {
                    OrderId = orderId,
                    OwnerAirline = Constants.Application.VSFlightCode
                },
                OrderStatus = string.Empty,
                SegmentDetails = segmentDetails,
                PointOfSale = isGDSbooking ? new PointOfSale()
                {
                    IATANumber = response?.TeardownContextResponse?.Pnr?.TicketNumberData1?.FirstOrDefault()?.IataNumber,
                    CurrencyCode = ticketingResponseList?.FirstOrDefault()?.TicketRequestResponse?.FirstOrDefault()?.TicketResponse?.FirstOrDefault()?.TicketInformation?.CurrencyCodeOfTotalFare,
                    CityCode = response?.TeardownContextResponse?.Pnr?.TicketNumberData1?.FirstOrDefault()?.OriginalTicketCity
                } : null,
                PaymentDetails = null,
                TimeLimits = TimeLimitsMapper.Air4ResponseToTimeLimitsMapper(response),
                ProductComponents = await ProductComponentsMapper.GetAmenities(segmentDetails, orderItems, cachedAmenities, mediator),
                TotalPrice = GetTotalPrice(ticketingResponseList),
            };
        }

        private static List<OrderSearchSegmentInfo> GetOrderSearchSegmentInfos(RetrieveResponseType response, bool isGDSbooking , EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType ticketingResponse)
        {
            List<OrderSearchSegmentInfo> orderSearchSegmentInfos = new();
            var dtiSegmentInfos = response?.TeardownContextResponse?.Pnr?.DtiSegmentInfo?.GroupBy(item => item.SegmentNumber).Select(q => q.FirstOrDefault()).Where(item => item.SegmentNumber != null).ToArray();

            List<string> totalSegmentNumber = new();
            if (response.TeardownContextResponse.Pnr.SegmentDl != null && response.TeardownContextResponse.Pnr.SegmentOa != null)
            {
                totalSegmentNumber = response.TeardownContextResponse.Pnr.SegmentDl.Select(x => x.SegmentNumber).Concat(response.TeardownContextResponse.Pnr.SegmentOa.Select(x => x.SegmentNumber)).OrderBy(x => x).ToList();
            }
            else if (response.TeardownContextResponse.Pnr.SegmentDl != null)
            {
                totalSegmentNumber = response.TeardownContextResponse.Pnr.SegmentDl.Select(x => x.SegmentNumber).ToList();
            }
            else if (response.TeardownContextResponse.Pnr.SegmentOa != null)
            {
                totalSegmentNumber = response.TeardownContextResponse.Pnr.SegmentOa.Select(x => x.SegmentNumber).ToList();
            }

            int index = 0;
            int prevIndex = -1;
            foreach (var segmentNo in totalSegmentNumber)
            {
                DtiSegmentInfo segmentDti = null;
                var segmentDl = response.TeardownContextResponse.Pnr.SegmentDl?.FirstOrDefault(seg => seg.SegmentNumber == segmentNo);
                var segmentOa = response.TeardownContextResponse.Pnr.SegmentOa?.FirstOrDefault(seg => seg.SegmentNumber == segmentNo);
                var segmentAdditionalInfos = response.TeardownContextResponse.Pnr.SegmentAdditionalInfo.FirstOrDefault(segAddInfo => segAddInfo.SegmentNumber == segmentNo);

                if (segmentDl != null && segmentDl.SegmentType != Constants.Application.ARNKSegmentType || segmentOa != null && segmentOa.SegmentType != Constants.Application.ARNKSegmentType)
                {
                    if (!isGDSbooking || ticketingResponse == null || ticketingResponse.TicketRequestResponse.FirstOrDefault().TicketResponse.FirstOrDefault().AlternateCouponFltHistory.Any(x => x.CityCode == (segmentDl != null ? segmentDl.DeparturePoint : segmentOa.DeparturePoint) && x.FlightDestinationAirport == (segmentDl != null ? segmentDl.ArrivalPoint : segmentOa.ArrivalPoint)))
                    {
                        if (segmentDl?.CurrentActionCode == Constants.Application.ScheduleChangeStatusCode || segmentOa?.CurrentActionCode == Constants.Application.ScheduleChangeStatusCode)
                        {
                            prevIndex = prevIndex == -1 ? index - 1 : prevIndex - 1;
                        }
                        if (prevIndex == -1)
                        {
                            segmentDti = dtiSegmentInfos?[index];
                        }
                        else
                        {
                            segmentDti = dtiSegmentInfos?[prevIndex];
                            prevIndex++;
                        }

                        OrderSearchSegmentInfo orderSearchSegmentInfo = new()
                        {
                            SegDlSegNo = segmentDl != null ? segmentDl?.SegmentNumber : segmentOa?.SegmentNumber,
                            DtiSegNo = segmentDti?.SegmentNumber,
                            SegAddInfoSegNo = segmentAdditionalInfos?.SegmentNumber
                        };

                        orderSearchSegmentInfos.Add(orderSearchSegmentInfo);
                        index++;
                    }
                }

            }
            return orderSearchSegmentInfos;
        }

        private static string GetDurationFormat(string duration, string format)
        {
            string durationHours = duration.Split(Constants.Application.Dot).First().PadLeft(2, '0');
            string durationMinutes = duration.Split(Constants.Application.Dot).Last().PadLeft(2, '0');

            if (duration != null)
                return format switch
                {
                    Constants.Application.DurationHMFormat => $"{durationHours}{Constants.Application.HourDesignator}{durationMinutes}{Constants.Application.MinutesDesignator}",
                    Constants.Application.DurationPTFormat => $"{Constants.Application.PTFormat}{durationHours}{Constants.Application.HourDesignator}{durationMinutes}{Constants.Application.MinutesDesignator}{Constants.Application.ZeroIndicator}{Constants.Application.SecondsDesignator}",
                    _ => $"{durationHours}{Constants.Application.Colon}{durationMinutes}",
                };

            return null;
        }
        private static CurrencyAmount GetTotalPrice(List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType> ticketingResponseList)
        {
            var currencyCode = ticketingResponseList.FirstOrDefault()?.TicketRequestResponse?.FirstOrDefault()?.TicketResponse?.FirstOrDefault()?.TicketInformation?.CurrencyCodeOfTotalFare;
            decimal totalPrice = 0;
            decimal totalFare = 0;

            foreach (var ticketingResponse in ticketingResponseList)
            {
                decimal.TryParse(ticketingResponse?.TicketRequestResponse?.FirstOrDefault()?.TicketResponse?.FirstOrDefault()?.TicketInformation?.TotalFare, out totalFare);
                totalPrice += totalFare;
            }

            if (totalPrice == 0)
                return null;

            return new CurrencyAmount()
            {
                BaseCurrency = currencyCode,
                MonetaryAmount = totalPrice.ToString() ?? string.Empty
            };
        }

        public static RetrieveRequestType PNRRetrieveRequestMapper(string pnr)
        {
            return new RetrieveRequestType()
            {
                SetupContextRequest = new SetupContextRequestType()
                {
                    RequestInfo = new RequestInfoType()
                    {
                        ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                        DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                        TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                        HostContextParams = new HostContextParamsType()
                        {
                            PoolType = ConfigReader.Get(Constants.Environment.PoolType)
                        }
                    },
                    RetrieveInfo = new RetrieveInfoType()
                    {
                        PassengerInfo = new PassengerInfoType()
                        {
                            RecordLocator = pnr
                        }
                    },
                    StampingInfo = new StampingInfoType()
                    {
                        UserAuthorizationData = new UserAuthorizationDataType()
                        {
                            AgentCityCode = ConfigReader.Get(Constants.Environment.AgentCityCode),
                            RoleIndicator = ConfigReader.Get(Constants.Environment.RoleIndicator),
                            SecurityId = ConfigReader.Get(Constants.Environment.SecurityId)
                        }
                    }
                }
            };
        }
    }
}