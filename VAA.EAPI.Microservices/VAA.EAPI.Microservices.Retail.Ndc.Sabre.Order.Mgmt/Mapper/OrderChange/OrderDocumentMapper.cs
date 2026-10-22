using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.PriceQuote;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public class OrderDocumentMapper
    {
        public static OrderDocument Mapper(OrderChangeRequestData orderChangeRequestData, AirTicketRS airTicketRS, GetReservationRS getReservationRS)
        {
            var deltaPNR = ((getReservationRS.Item as ReservationPNRB).PassengerReservation.Segments.Segment.First().Item as SegmentTypePNRBSegmentAir).AirlineRefId[^6..];
            string pnr = (getReservationRS.Item as ReservationPNRB).BookingDetails.RecordLocator;
            string orderId = $"VS932{pnr}{DateTime.UtcNow:yy}";
            var warningList = new List<Warning>();
            orderChangeRequestData.DataList.SegmentDetails = GetSegmentDetails(orderChangeRequestData, getReservationRS);
            orderChangeRequestData.DataList.CustomerDetails = GetCustomerDetails(orderChangeRequestData, (ReservationPNRB)getReservationRS.Item);

            return new OrderDocument()
            {
                BookingReference = new()
                {
                    ExternalSystem = "1S",
                    ExternalRecordLocator = deltaPNR,
                    RLOC = orderChangeRequestData.Rloc,
                    RLOCCreateDate = DateTime.UtcNow.ToString(Constants.Application.DateFormat),
                    RLOCCreateDateTime = (getReservationRS.Item as ReservationPNRB).BookingDetails.CreationTimestamp.ToString(Constants.Application.TimeOnlyFormat)
                },
                CreationDateTime = new()
                {
                    UtcDateTime = MapUtcDateTime((getReservationRS.Item as ReservationPNRB).BookingDetails.CreationTimestamp).ToString(Constants.Application.ZuluDateTimeFormat),
                },
                DocType = Constants.Application.OrderDocType,
                Id = orderId,
                PartitionKey = orderId,
                Parties = orderChangeRequestData.Parties,
                PointOfSale = orderChangeRequestData.Pos,
                TimeLimits = orderChangeRequestData.TimeLimits,
                OrderNumber = new()
                {
                    OrderId = orderId,
                    OwnerAirline = Constants.Application.OwnerAirline
                },
                OrderRemarks = orderChangeRequestData.OrderRemarks,
                OrderItems = MapOrderItems(orderChangeRequestData, orderId, airTicketRS, warningList, getReservationRS),
                OrderStatus = Constants.Application.OrderStatusConfirmed,
                Warnings = warningList.Any() ? warningList : null,
                DataList = orderChangeRequestData.DataList,
                ChangeDateTime = new() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) },
                ChangeType = Domain.ChangeTypeEnum.DocumentExchanged
            };
        }

        private static IEnumerable<OrderItems> MapOrderItems(OrderChangeRequestData orderChangeRequestData, string orderId, AirTicketRS airTicketRS, List<Warning> warningList, GetReservationRS orderRs)
        {

            var orderItemList = new List<OrderItems>();
            var reservation = orderRs?.Item as ReservationPNRB;
            int index = 0;
            var paymentDetails = orderChangeRequestData?.TransactionId?.Select(item =>
            {
                return new PaymentDetails()
                {
                    PaymentReferenceId = item
                };
            });

            var firstPayment = orderChangeRequestData?.PaymentDetails?.FirstOrDefault();
            if (firstPayment?.Amount?.MonetaryAmount == "0.00")
            {
                paymentDetails = paymentDetails.Concat(new[]{new PaymentDetails()
                {
                    Amount = new CurrencyAmount()
                    {
                        MonetaryAmount = "0.00",
                        BaseCurrency = firstPayment.Amount.BaseCurrency
                    },
                    PaymentCommitmentDateTime = DateTime.UtcNow.ToString(Constants.Application.InputDateTimeFormat),
                    PaymentReferenceId = GenerateTransactionId(),
                    PaymentProcessingStatus = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL
                }});
            }

            foreach (var offer in orderChangeRequestData.SelectedOffers.First().OfferItems)
            {
                var ptc = offer.Services.FirstOrDefault().ServiceBeneficiaries.Select(y =>
                {
                    return orderChangeRequestData.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == y);
                });
                var offerItemPricing = offer.OfferitemPricing.FirstOrDefault();

                orderItemList.Add(new OrderItems()
                {
                    OrderItemId = $"{orderId}-{++index}",
                    OwnerAirline = Constants.Application.OwnerAirline,
                    Services = GetServices(offer.Services, orderChangeRequestData.DataList.SegmentDetails),
                    PriceDetails = GetPriceDetails(offer.OfferitemPricing.First().SalePrice, offer.OfferitemPricing.First().FareCalculationText),
                    SalePriceDifference = GetPriceDetails(offer.OfferitemPricing.First().SalePriceDifference, offer.OfferitemPricing.First().FareCalculationText),
                    OriginalSalePrice = GetPriceDetails(offer.OfferitemPricing.First().OriginalSalePrice, offer.OfferitemPricing.First().FareCalculationText),
                    AddCollectPrice = offerItemPricing?.AddCollectPrice,
                    RefundPrice = offerItemPricing?.RefundPrice,
                    PenaltyPrices = offerItemPricing?.PenaltyPrices,
                    ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                    PaymentDetails = paymentDetails,
                    AccountableDocumentRefs = airTicketRS is not null
                        ? MapAccountableDocumentRefs(ptc, airTicketRS, orderChangeRequestData.DataList)
                        : orderChangeRequestData.IsReshopDisruption && reservation is not null
                            ? MapAccountableDocumentRefs(ptc, reservation)
                            : null
                });
            }

            if ((orderChangeRequestData.IsChangeReshopConfirmHoldBooking || orderChangeRequestData.IsChangeReshopHoldBooking) &&!orderChangeRequestData.DataList.OrderInstructions.IsNullOrEmpty())
            {
                warningList.Add(new()
                {
                    Code = "173",
                    Message = "SSR needs to be rebooked for the new itinerary",
                });
            }
            else
            {
                var passengersWithSSRs = (orderRs.Item as ReservationPNRB)?.PassengerReservation?.Passengers.Passenger.Where(x => x.SpecialRequests?.SpecialMealRequest != null || x.SpecialRequests?.WheelchairRequest != null || x.SpecialRequests?.GenericSpecialRequest != null);
                if (passengersWithSSRs.Any())
                {
                    if (!orderChangeRequestData.DataList.OrderInstructions.IsNullOrEmpty())
                    {
                        foreach (var oi in orderChangeRequestData.DataList.OrderInstructions?.Where(x => x.ItemText.Contains(Constants.Application.FlightService)).GroupBy(x => x.ItemCode))
                        {
                            var instId = 1;
                            List<OrderInstructions> ois = [];

                            foreach (var ssr in oi)
                            {
                                var customer = orderChangeRequestData.DataList.CustomerDetails.First(x => x.CustId == ssr.ItemText.Split("/")[1]);
                                var seg = orderChangeRequestData.DataList.SegmentDetails.First(x => x.SegmentId == ssr.ItemText.Split('/')[2]);

                                var passenger = customer.Ptc == "INF" ? passengersWithSSRs.First(p => p.SpecialRequests?.APISRequest != null && p.SpecialRequests.APISRequest.Any(a => a.DOCSEntry != null && a.DOCSEntry.Forename.Is(customer.FirstName) && a.DOCSEntry.Surname.Is(customer.LastName))) : passengersWithSSRs.First(p => p.FirstName.Is(customer.FirstName) && p.LastName.Is(customer.LastName));

                                var mealSSRs = passenger.SpecialRequests?.SpecialMealRequest != null ? passenger.SpecialRequests.SpecialMealRequest.Where(m => m.MealType == oi.Key && m.BoardCity.Is(seg.OriginAirportCode) && m.OffCity.Is(seg.DestinationAirportCode) && DateTime.TryParse(Convert.ToString(m.FlightDate), out var flightDate) && DateTime.TryParse(Convert.ToString(seg.ScheduledDepartureTime?.LocalDateTime), out var schedDateTime) && flightDate.Date == schedDateTime.Date) : null;
                                var wheelChairSSRs = passenger.SpecialRequests?.WheelchairRequest != null ? passenger.SpecialRequests.WheelchairRequest.Where(m => m.WheelchairCode.ToString() == oi.Key && m.BoardCity.Is(seg.OriginAirportCode) && m.OffCity.Is(seg.DestinationAirportCode) && DateTime.TryParse(Convert.ToString(m.FlightDate), out var flightDate) && DateTime.TryParse(Convert.ToString(seg.ScheduledDepartureTime?.LocalDateTime), out var schedDateTime) && flightDate.Date == schedDateTime.Date) : null;
                                var generalSSRs = passenger.SpecialRequests?.GenericSpecialRequest != null ? passenger.SpecialRequests.GenericSpecialRequest.Where(g => g.Code == oi.Key && g.FullText.Contains($"{seg.OriginAirportCode}{seg.DestinationAirportCode}{seg.MarketingFlightNumber.PadLeft(4, '0')}{seg.ClassOfService}{Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}")) : null;

                                if (mealSSRs != null)
                                {
                                    foreach (var mealSSR in mealSSRs)
                                    {
                                        if (passenger.SpecialRequests.SpecialMealRequest.Any(m => m.MealType == oi.Key && m.BoardCity.Is(seg.OriginAirportCode) && m.OffCity.Is(seg.DestinationAirportCode) && DateTime.TryParse(Convert.ToString(m.FlightDate), out var flightDate) && DateTime.TryParse(Convert.ToString(seg.ScheduledDepartureTime?.LocalDateTime), out var schedDateTime) && flightDate.Date == schedDateTime.Date && m.ActionCode == "KK"))
                                            ois.Add(new()
                                            {
                                                InstructionId = $"OI{instId++}",
                                                ItemCode = mealSSR.MealType,
                                                ItemType = OrderInstructions.ItemTypes.SSREnum,
                                                ItemText = $"FlightService/{ssr.ItemText.Split("/")[1]}/{seg.SegmentId}/meal/{ssr.ItemText.Split("/").Last()}"
                                            });

                                        else if (passenger.SpecialRequests.SpecialMealRequest.Any(m => m.MealType == oi.Key && m.BoardCity.Is(seg.OriginAirportCode) && m.OffCity.Is(seg.DestinationAirportCode) && DateTime.TryParse(Convert.ToString(m.FlightDate), out var flightDate) && DateTime.TryParse(Convert.ToString(seg.ScheduledDepartureTime?.LocalDateTime), out var schedDateTime) && flightDate.Date == schedDateTime.Date && (m.ActionCode == "NO" || m.ActionCode == "UN")))
                                            warningList.Add(new()
                                            {
                                                Code = "186",
                                                Details = ssr.ItemCode,
                                                Message = $"Failed to add the SSR details for {ssr.ItemText.Split("/")[1]} for seg {ssr.ItemText.Split("/")[2]}",
                                            });

                                        else
                                            warningList.Add(new()
                                            {
                                                Code = "182",
                                                Message = $"Failed to add the SSR details for {ssr.ItemText.Split("/")[1]} for seg {ssr.ItemText.Split("/")[2]}",
                                            });

                                    }
                                }

                                if (wheelChairSSRs != null)
                                {
                                    foreach (var wheelChairSSR in wheelChairSSRs)
                                    {
                                        if (passenger.SpecialRequests.WheelchairRequest.Any(m => m.WheelchairCode.ToString() == oi.Key && m.BoardCity.Is(seg.OriginAirportCode) && m.OffCity.Is(seg.DestinationAirportCode) && DateTime.TryParse(Convert.ToString(m.FlightDate), out var flightDate) && DateTime.TryParse(Convert.ToString(seg.ScheduledDepartureTime?.LocalDateTime), out var schedDateTime) && flightDate.Date == schedDateTime.Date && m.ActionCode == "KK"))

                                            ois.Add(new()
                                            {
                                                InstructionId = $"OI{instId++}",
                                                ItemCode = wheelChairSSR.WheelchairCode.ToString(),
                                                ItemType = OrderInstructions.ItemTypes.SSREnum,
                                                ItemText = $"FlightService/{ssr.ItemText.Split("/")[1]}/{seg.SegmentId}/wheelChair/{ssr.ItemText.Split("/").Last()}"
                                            });

                                        else if (passenger.SpecialRequests.WheelchairRequest.Any(m => m.WheelchairCode.ToString() == oi.Key && m.BoardCity.Is(seg.OriginAirportCode) && m.OffCity.Is(seg.DestinationAirportCode) && DateTime.TryParse(Convert.ToString(m.FlightDate), out var flightDate) && DateTime.TryParse(Convert.ToString(seg.ScheduledDepartureTime?.LocalDateTime), out var schedDateTime) && flightDate.Date == schedDateTime.Date && (m.ActionCode == "NO" || m.ActionCode == "UN")))
                                            warningList.Add(new()
                                            {
                                                Code = "186",
                                                Details = ssr.ItemCode,
                                                Message = $"Failed to add the SSR details for {ssr.ItemText.Split("/")[1]} for seg {ssr.ItemText.Split("/")[2]}",
                                            });

                                        else
                                            warningList.Add(new()
                                            {
                                                Code = "182",
                                                Message = $"Failed to add the SSR details for {ssr.ItemText.Split("/")[1]} for seg {ssr.ItemText.Split("/")[2]}",
                                            });

                                    }
                                }

                                if (generalSSRs != null)
                                {
                                    foreach (var generalSSR in generalSSRs.Where(x => x.ActionCode == "NN" || x.ActionCode == "HN"))
                                    {
                                        if (generalSSRs.Any(s => s.Code == generalSSR.Code && s.FullText == generalSSR.FullText.Replace("NN", "KK").Replace("HN", "KK") && s.ActionCode == "KK"))
                                            ois.Add(new()
                                            {
                                                InstructionId = $"OI{instId++}",
                                                ItemCode = generalSSR.Code,
                                                ItemType = OrderInstructions.ItemTypes.SSREnum,
                                                ItemText = $"FlightService/{ssr.ItemText.Split("/")[1]}/{seg.SegmentId}/general/{ssr.ItemText.Split("/").Last()}"
                                            });

                                        else if (passenger.SpecialRequests.GenericSpecialRequest.Any(g => g.Code == oi.Key && g.FullText.Contains($"{seg.OriginAirportCode}{seg.DestinationAirportCode}{seg.MarketingFlightNumber.PadLeft(4, '0')}{seg.ClassOfService}{Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}") && (g.ActionCode == "NO" || g.ActionCode == "UN")))
                                            warningList.Add(new()
                                            {
                                                Code = "186",
                                                Details = ssr.ItemCode,
                                                Message = $"Failed to add the SSR details for {ssr.ItemText.Split("/")[1]} for seg {ssr.ItemText.Split("/")[2]}",
                                            });

                                        else
                                            warningList.Add(new()
                                            {
                                                Code = "182",
                                                Message = $"Failed to add the SSR details for {ssr.ItemText.Split("/")[1]} for seg {ssr.ItemText.Split("/")[2]}",
                                            });

                                    }
                                }

                                if (mealSSRs == null && wheelChairSSRs == null && generalSSRs == null)
                                    warningList.Add(new()
                                    {
                                        Code = "185",
                                        Details = ssr.ItemCode,
                                        Message = $"Failed to add the SSR details for {ssr.ItemText.Split("/")[1]} for seg {ssr.ItemText.Split("/")[2]}",
                                    });

                            }

                            orderChangeRequestData.DataList.OrderInstructions = ois;

                            if (ois.Count != 0)
                            {
                                orderItemList.Add(new OrderItems()
                                {
                                    OrderItemId = $"{orderId}-{orderItemList.Count + 1}",
                                    OwnerAirline = Constants.Application.OwnerAirline,
                                    Services = GetServices(ois),
                                    ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                    PriceDetails =
                                    [
                                        new()
                                {
                                    Amount = new()
                                    {
                                        BaseCurrency = orderChangeRequestData.SelectedOffers.First().OfferItems.First().OfferitemPricing.First().SalePrice.First().Amount.BaseCurrency ,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }
                                    ]
                                });
                            }
                        }
                    }
                }
            }

            return orderItemList;
        }

        private static IEnumerable<OrderService> GetServices(IEnumerable<OrderInstructions> orderInstructions)
        {
            return orderInstructions.Select(oi =>
            {
                var ssrInfo = oi.ItemText.Split('/');

                return new OrderService()
                {
                    ServiceId = $"SV{1}",
                    SegmentRefIds = [ssrInfo[2]],
                    ServiceBeneficiaries = [ssrInfo[1]],
                    OrderInstructionRefs = [oi.InstructionId]
                };
            });
        }

        private static IEnumerable<AccountableDocumentRef> MapAccountableDocumentRefs(IEnumerable<Customer> customers, AirTicketRS airTicketRS, DataList dataList)
        {
            var tickets = customers.Select(ptc =>
            {
                var customer = dataList.CustomerDetails.FirstOrDefault(x => x.CustId == ptc.CustId);
                var ticket = airTicketRS.Summary.FirstOrDefault(x => x.LastName == customer.LastName && x.FirstName == customer.FirstName);
                return new { ticket.DocumentNumber, customer.CustId };

            });

            return tickets?.Select(ticket =>
            {
                return new AccountableDocumentRef()
                {
                    DocumentNumber = ticket.DocumentNumber,
                    CustomerRefId = ticket.CustId,
                    DocumentType = AccountableDocumentRef.DocumentTypes.TEnum,
                };
            });
        }

        private static IEnumerable<Price> GetPriceDetails(List<Price> salePrice, string farecalculationtext)
        {
            return salePrice.Select(sp =>
                new Price()
                {
                    Amount = sp.Amount,
                    PriceComponentType = sp.PriceComponentType,
                    BaseCurrencyAmount = sp.BaseCurrencyAmount,
                    TaxBreakup = sp.TaxBreakup,
                    FareCalculation = farecalculationtext,
                    EquivalentTaxBreakup = sp?.EquivalentTaxBreakup
                });
        }

        private static IEnumerable<OrderService> GetServices(IEnumerable<OfferService> offerServices, IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.Segment> segmentDetails)
        {
            return offerServices.SelectMany(x => x.FlightSegmentsRefIds).Select((seg, index) =>
            {
                var service = offerServices.First(x => x.FlightSegmentsRefIds.Contains(seg));

                return new OrderService()
                {
                    ServiceId = $"SV{++index}",
                    Product = new Product()
                    {
                        ProductType = Product.ProductTypes.FlightEnum
                    },
                    PenaltyRef = service.PenaltyRef,
                    BaggageRefIds = service.BaggageRefIds,
                    PriceClassRefIds = service.PriceClassRefIds,
                    SegmentRefIds = new[] { seg },
                    ServiceBeneficiaries = service.ServiceBeneficiaries,
                    ServiceDefinitionRefIds = new[] { segmentDetails.FirstOrDefault(s => s.SegmentId == seg).ServiceDefinitionsRefID }
                };
            });
        }

        private static IEnumerable<Segment> GetSegmentDetails(OrderChangeRequestData orderChangeRequestData, GetReservationRS response)
        {
            var flightSegments = (response.Item as ReservationPNRB)?.PassengerReservation?.Segments?.Segment;
            var priceQuote = (PriceQuoteInfoGetResponse)response.PriceQuote.Item;

            foreach (var segment in orderChangeRequestData.DataList.SegmentDetails)
            {
                var segmentScheduledDepartureDate = DateTime.Parse(segment.ScheduledDepartureTime.LocalDateTime).Date;
                var segmentScheduledArrivalDate = DateTime.Parse(segment.ScheduledArrivalTime.LocalDateTime).Date;

                var matchSegment = flightSegments?.FirstOrDefault(x =>
                {
                    var airSegments = x.Item as SegmentTypePNRBSegmentAir;
                    if (airSegments == null) return false;

                    var airSegmentsDepartureDate = DateTime.Parse(airSegments.DepartureDateTime).Date;
                    var airSegmentsArrivalDate = DateTime.Parse(airSegments.ArrivalDateTime).Date;

                    return airSegments.DepartureAirport == segment.OriginAirportCode && airSegments.ArrivalAirport == segment.DestinationAirportCode && airSegmentsDepartureDate == segmentScheduledDepartureDate && airSegmentsArrivalDate == segmentScheduledArrivalDate;
                })?.Item as SegmentTypePNRBSegmentAir;

                var fareBasisCode = priceQuote == null ? null : (from pq in priceQuote.Details
                                                                 from segInfos in pq.SegmentInfo
                                                                 where segInfos.Flight.Departure.CityCode.Value == matchSegment.DepartureAirport && segInfos.Flight.Arrival.CityCode.Value == matchSegment.ArrivalAirport
                                                                 select segInfos.FareBasis).First();

                segment.Status = new() { Code = matchSegment?.ActionCode , DeliveryStatusCode = matchSegment.isPast ? "Delivered" : null };
                segment.FareBasis = fareBasisCode;
                segment.ScheduledDepartureTime = new() { LocalDateTime = matchSegment?.DepartureDateTime };
                segment.ScheduledArrivalTime = new() { LocalDateTime = matchSegment?.ArrivalDateTime };
            }
            return orderChangeRequestData.DataList.SegmentDetails;
        }

        private static DateTime MapUtcDateTime(DateTime creationDateTimeStamp)
        {
            var cstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(creationDateTimeStamp, cstTimeZone);
        }

        private static String GenerateTransactionId()
        {
            long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            Random rand = new Random();

            int dRandom = rand.Next(00000000, 99999999);
            string strFinalNum = Convert.ToString(unixTimestamp) + Convert.ToString(dRandom);

            return strFinalNum;
        }

        public static OrderDocument DisruptionOrderDocMapper(GetReservationRS getReservationRS, OrderChangeRequestData request)
        {
            var orderId = $"VS932{request.Rloc}{DateTime.UtcNow:yy}";
            var reservation = (ReservationPNRB)getReservationRS.Item;
            var priceQuote = getReservationRS.PriceQuote.Item != null ? (PriceQuoteInfoGetResponse)getReservationRS.PriceQuote.Item : null;
            var warnings = new List<Warning>();

            var dataList = new DataList()
            {
                CustomerDetails = [],
                OriginDestinationAssociations = [],
                OrderInstructions = [],
                SegmentDetails = [],
                PriceClass = [],
                Journey = []
            };

            if (reservation.PassengerReservation.Segments?.Segment != null)
                MapOriginDestinationAssociations(reservation.PassengerReservation.Segments.Segment, dataList, priceQuote);

            dataList.Journey = MapJourney(dataList.OriginDestinationAssociations, dataList.SegmentDetails);
            dataList.PriceClass = MapPriceClass(dataList.SegmentDetails);
            request.DataList.CustomerDetails = GetCustomerDetails(request, reservation);
            dataList.CustomerDetails = request.DataList.CustomerDetails;

            UpdateDataList(dataList);

            return new OrderDocument()
            {
                BookingReference = new()
                {
                    ExternalSystem = "1S",
                    ExternalRecordLocator = ((SegmentTypePNRBSegmentAir)reservation.PassengerReservation.Segments?.Segment?.FirstOrDefault()?.Item)?.AirlineRefId.Split("*")?.LastOrDefault(),
                    RLOC = request.Rloc,
                    RLOCCreateDate = reservation.BookingDetails.CreationTimestamp.ToString(Constants.Application.DateFormat),
                    RLOCCreateDateTime = reservation.BookingDetails.CreationTimestamp.ToString(Constants.Application.TimeOnlyFormat)
                },
                CreationDateTime = new()
                {
                    UtcDateTime = MapUtcDateTime(reservation.BookingDetails.CreationTimestamp).ToString(Constants.Application.ZuluDateTimeFormat),
                },
                DataList = dataList,
                DocType = Constants.Application.OrderDocType,
                Id = orderId,
                PartitionKey = orderId,
                Parties = request.Parties,
                OrderNumber = new()
                {
                    OrderId = orderId,
                    OwnerAirline = Constants.Application.OwnerAirline
                },
                OrderItems = MapOrderItems(orderId, reservation, priceQuote, dataList, warnings),
                OrderStatus = reservation.PassengerReservation.TicketingInfo?.TicketDetails?.Length > 0
                    ? Constants.Application.OrderStatusConfirmed
                    : string.Empty,
                Warnings = warnings
            };
        }

        private static void MapOriginDestinationAssociations(SegmentTypePNRBSegment[] segments, DataList dataList, PriceQuoteInfoGetResponse priceQuote)
        {
            var originDestinationAssociationsList = new List<OriginDestinationAssociation>();
            string departureAirport = "";
            string arrivalAirport = "";
            string departureDateTime = "";
            var segmentDetailsList = dataList.SegmentDetails.ToList();

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Item.GetType().Name == "Arunk")
                    continue;

                var segment = (SegmentTypePNRBSegmentAir)segments[i].Item;
                var index = originDestinationAssociationsList.Count;
                var odRefId = $"{Constants.Application.OriginDestinationIdPrefix}{index + 1}";
                var ond = new OriginDestinationAssociation();
                var connectionDuration = ((!segment.outboundConnection && !segment.inboundConnection) || (segment.outboundConnection && !segment.inboundConnection)) ? null : DateTime.Parse(segment.DepartureDateTime).Subtract(DateTime.Parse(((SegmentTypePNRBSegmentAir)segments[i - 1].Item).ArrivalDateTime)).TotalMinutes.ToString();

                MapSegmentDetails(segment, odRefId, segmentDetailsList, priceQuote, connectionDuration);

                if (segment.outboundConnection)
                {
                    departureAirport = segment.DepartureAirport;
                    departureDateTime = segment.DepartureDateTime;
                    continue;
                }

                if (segment.inboundConnection)
                {
                    arrivalAirport = segment.ArrivalAirport;
                }

                // Handle direct flights (both inbound and outbound are false)
                if (!segment.outboundConnection && !segment.inboundConnection)
                {
                    ond = new OriginDestinationAssociation
                    {
                        OdLabel = odRefId,
                        OriginDestination = new()
                        {
                            OriginAirport = segment.DepartureAirport,
                            DestinationAirport = segment.ArrivalAirport,
                            DepartureDate = DateTime.Parse(segment.DepartureDateTime).ToString(Constants.Application.DateOnlyFormat)
                        }
                    };
                }
                else
                {
                    ond = new OriginDestinationAssociation
                    {
                        OdLabel = odRefId,
                        OriginDestination = new()
                        {
                            OriginAirport = departureAirport,
                            DestinationAirport = arrivalAirport,
                            DepartureDate = departureDateTime
                        }
                    };
                }
                originDestinationAssociationsList.Add(ond);
            }

            dataList.SegmentDetails = segmentDetailsList;
            dataList.OriginDestinationAssociations = originDestinationAssociationsList;
        }

        private static void MapSegmentDetails(SegmentTypePNRBSegmentAir segment, string OdRefId, List<Segment> segmentList, PriceQuoteInfoGetResponse priceQuote, string connectionDuration)
        {
            var fareBasisCode = priceQuote == null ? null : (from pq in priceQuote?.Details
                                                             from segInfos in pq.SegmentInfo
                                                             where segInfos.Flight.Departure.CityCode.Value == segment.DepartureAirport && segInfos.Flight.Arrival.CityCode.Value == segment.ArrivalAirport
                                                             select segInfos.FareBasis).First();

            segmentList.Add(new Segment()
            {
                OdRefId = OdRefId,
                SegmentId = $"{Constants.Application.SegmentIdPrefix}{segmentList.Count + 1}",
                AirCraft = segment.EquipmentType,
                OriginAirportCode = segment.DepartureAirport,
                DestinationAirportCode = segment.ArrivalAirport,
                ScheduledDepartureTime = new()
                {
                    LocalDateTime = segment.DepartureDateTime
                },
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = segment.ArrivalDateTime
                },
                MarketingCarrier = segment.MarketingAirlineCode,
                MarketingFlightNumber = segment.MarketingFlightNumber?.TrimStart('0'),
                OperatingCarrier = segment.OperatingAirlineCode,
                OperatingFlightNumber = segment.OperatingFlightNumber?.TrimStart('0'),
                ClassOfService = segment.ClassOfService,
                Status = new() { Code = segment.ActionCode },
                CabinType = new()
                {
                    CabinCode = GetCabinCode(segment.Cabin.Code),
                    CabinName = segment.Cabin.Name
                },
                FareBasis = fareBasisCode,
                FlightDuration = GetDuration(segment.HiddenStop, segment.ElapsedTime ?? "0"),
                ConnectionDuration = connectionDuration,
                Legs = AddLegs(segment),
                ScheduleChangeIndicator = segment.ScheduleChangeIndicator
            });
        }

        private static List<Legs> AddLegs(SegmentTypePNRBSegmentAir segment)
        {
            List<Legs> legs =
            [
                new Legs()
                {
                    LegId = $"{Constants.Application.LegIdPrefix}{segment?.sequence}",
                    ScheduledArrivalTime = new() { LocalDateTime = segment?.HiddenStop is not null ? segment?.HiddenStop.First().ArrivalDateTime : segment.ArrivalDateTime },
                    ScheduledDepartureTime = new() { LocalDateTime =  segment.DepartureDateTime },
                    OriginAirportCode = segment.DepartureAirport,
                    OperatingCarrier = segment.OperatingAirlineCode,
                    DestinationAirportCode = segment?.HiddenStop is not null ? segment.HiddenStop.First().Airport : segment.ArrivalAirport,
                    CarrierAircraftTypeCode = segment.EquipmentType,
                    CarrierAircraftTypeName = segment.EquipmentType,
                    LegSequenceNumber = $"{segment?.sequence}",
                    LegDuration = GetDuration(segment?.HiddenStop, segment.ElapsedTime ?? "0"),
                    StopOverDuration = segment?.HiddenStop is not null  ? GetDuration(segment?.HiddenStop) : null

                },
            ];
            if (segment?.HiddenStop is not null)
            {
                legs.Add(new Legs()
                {
                    LegId = $"{Constants.Application.LegIdPrefix}{++segment.sequence}",
                    ScheduledArrivalTime = new() { LocalDateTime = segment.HiddenStop.First().ArrivalDateTime },
                    ScheduledDepartureTime = new() { LocalDateTime = segment.HiddenStop.First().DepartureDateTime },
                    OriginAirportCode = segment.HiddenStop.First().Airport,
                    OperatingCarrier = segment.OperatingAirlineCode,
                    DestinationAirportCode = segment.ArrivalAirport,
                    CarrierAircraftTypeCode = segment.HiddenStop.First().EquipmentType,
                    CarrierAircraftTypeName = segment.HiddenStop.First().EquipmentType,
                    LegSequenceNumber = $"{segment.sequence}",
                    LegDuration = GetDuration(segment.HiddenStop, segment.ElapsedTime ?? "0")
                });

            }

            return legs;
        }

        private static string GetDuration(AirTypeHiddenStop[] hiddenStops, string elapsedTime = "0")
        {
            if (hiddenStops is null)
            {
                return TimeSpan.Parse(elapsedTime.Replace(".", ":")).ToString(Constants.Application.DurationFormat);
            }
            else
            {
                string[] timeParts = elapsedTime.Replace(".", ":").Split(':');
                double totalMinutes = timeParts.Length == 1 ? 0 : GetTotalMintues(timeParts);
                totalMinutes += hiddenStops.Sum(x => DateTime.Parse(x.DepartureDateTime).Subtract(DateTime.Parse(x.ArrivalDateTime)).TotalMinutes);

                return TimeSpan.FromMinutes(totalMinutes).ToString(Constants.Application.DurationFormat);
            }
        }

        private static double GetTotalMintues(string[] timeParts)
        {
            int hours = int.Parse(timeParts[0]);
            int days = 0;
            if (hours >= 24)
            {
                days = hours / 24;
                hours = hours % 24;
            }

            string validTimeSpanString = $"{days:D2}:{hours:D2}:{timeParts[1]}:00";

            TimeSpan timeSpan = TimeSpan.Parse(validTimeSpanString);
            double totalMinutes = timeSpan.TotalMinutes;
            return totalMinutes;
        }

        private static string GetCabinCode(string classType)
        {
            return classType switch
            {
                "C" => "2",
                "J" => "2",
                "W" => "4",
                "Y" => "5",
                _ => string.Empty
            };
        }

        private static IEnumerable<Journey> MapJourney(IEnumerable<OriginDestinationAssociation> ond, IEnumerable<Segment> segments)
        {
            return ond.Select((ond, index) =>
            {
                var flightSegmentRefIds = segments.Where(x => x.OdRefId == ond.OdLabel).Select(x => x.SegmentId);
                var totalMinutes = segments.Where(x => flightSegmentRefIds.Contains(x.SegmentId)).Sum(x => TimeSpan.Parse(x.FlightDuration).TotalMinutes);
                totalMinutes += segments.Where(x => x.ConnectionDuration != null).Sum(x => int.Parse(x.ConnectionDuration));

                return new Journey
                {
                    JourneyId = $"{Constants.Application.JourneyIdPrefix}{index + 1}",
                    FlightSegmentRefIds = flightSegmentRefIds,
                    Duration = TimeSpan.FromMinutes(totalMinutes).ToString(@"hh\:mm")
                };
            });
        }

        private static void UpdateDataList(DataList dataList)
        {
            var originDestinationAssociationsList = dataList.OriginDestinationAssociations.ToList();
            foreach (var ond in originDestinationAssociationsList)
            {
                ond.FlightSegmentRefIds = dataList.SegmentDetails.Where(x => x.OdRefId == ond.OdLabel).Select(x => x.SegmentId);
                ond.JourneyRefIds = dataList.Journey.Where(x => x.FlightSegmentRefIds.SequenceEqual(ond.FlightSegmentRefIds)).Select(x => x.JourneyId).ToList();
            }
            dataList.OriginDestinationAssociations = originDestinationAssociationsList;
        }

        private static IEnumerable<Customer> GetCustomerDetails(OrderChangeRequestData request, ReservationPNRB reservation)
        {
            foreach (var cust in request.DataList.CustomerDetails)
            {
                var matchingCust = reservation.PassengerReservation.Passengers.Passenger.First(x => x.FirstName == cust.FirstName && x.LastName == cust.LastName);

                if (matchingCust != null)
                {
                    cust.DateOfBirth = cust.Ptc == Constants.Application.Infant ? reservation.PassengerReservation.Passengers.Passenger.FirstOrDefault(p => (bool)p.SpecialRequests?.APISRequest?.Any(a => a?.DOCSEntry?.Forename == cust.FirstName && a?.DOCSEntry?.Surname == cust.LastName && a?.DOCSEntry?.Gender != null))?.SpecialRequests?.APISRequest?.FirstOrDefault(a => a.DOCSEntry?.Forename == cust.FirstName && a?.DOCSEntry?.Surname == cust.LastName && a?.DOCSEntry?.Gender != null)?.DOCSEntry?.DateOfBirth!.ToString(Constants.Application.DateOnlyFormat) : matchingCust.SpecialRequests?.APISRequest?.First(x => x.DOCSEntry != null).DOCSEntry?.DateOfBirth!.ToString(Constants.Application.DateOnlyFormat);
                    cust.Gender = cust.Ptc == Constants.Application.Infant ? reservation.PassengerReservation.Passengers.Passenger.FirstOrDefault(p => (bool)p.SpecialRequests?.APISRequest?.Any(a => a?.DOCSEntry?.Forename == cust.FirstName && a?.DOCSEntry?.Surname == cust.LastName && a?.DOCSEntry?.Gender != null))?.SpecialRequests?.APISRequest?.FirstOrDefault(a => a.DOCSEntry?.Forename == cust.FirstName && a?.DOCSEntry?.Surname == cust.LastName && a?.DOCSEntry?.Gender != null)?.DOCSEntry?.Gender.Replace("I", "") : matchingCust.SpecialRequests?.APISRequest?.First(x => x.DOCSEntry != null).DOCSEntry.Gender;
                }
            }
            return request.DataList.CustomerDetails;
        }

        private static IEnumerable<PriceClass> MapPriceClass(IEnumerable<Segment> segments)
        {
            return segments.DistinctBy(x => x.FareBasis).Select((segment, index) => new PriceClass()
            {
                Id = $"PC{++index}",
                FareBasisCode = segment.FareBasis,
                CabinType = segment.CabinType,
            });
        }

        private static IEnumerable<OrderItems> MapOrderItems(string orderId, ReservationPNRB reservation, PriceQuoteInfoGetResponse priceQuote, DataList dataList, List<Warning> warnings)
        {
            var orderItems = reservation.PassengerReservation.Passengers.Passenger.GroupBy(passenger => passenger.passengerType).Select((group, index) => new OrderItems
                {
                    OrderItemId = $"{orderId}-{++index}",
                    ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                    OwnerAirline = Constants.Application.OwnerAirline,
                    AccountableDocumentRefs = MapAccountableDocumentRefs(group.Key == Constants.Application.CNN ? Constants.Application.Child : group.Key == Constants.Application.GBESabreCode ? Constants.Application.GBECode : group.Key, reservation, dataList),
                    Services = MapServices(group.Key == Constants.Application.CNN ? Constants.Application.Child : group.Key == Constants.Application.GBESabreCode ? Constants.Application.GBECode : group.Key, dataList),
                    PriceDetails = MapPriceDetails(group.Key, priceQuote)
                }).ToList();

            var isSsrAvailable = reservation.PassengerReservation.Passengers.Passenger.Any(x => x.SpecialRequests?.SpecialMealRequest is not null || x.SpecialRequests?.WheelchairRequest is not null || x.SpecialRequests?.GenericSpecialRequest is not null);

            if (isSsrAvailable)
            {
                List<OrderInstructions> ois = new();
                var instId = 1;

                if (reservation.PassengerReservation.Passengers.Passenger.Any(x => x.SpecialRequests?.SpecialMealRequest is not null))
                {
                    var ssrs = reservation.PassengerReservation.Passengers.Passenger.Where(p => p.SpecialRequests?.SpecialMealRequest != null)?.SelectMany(x => x.SpecialRequests.SpecialMealRequest.Where(s => s.ActionCode == "NN" || s.ActionCode == "HN"))?.GroupBy(x => x.MealType);

                    foreach (var ssr in ssrs)
                    {
                        foreach (var s in ssr)
                        {
                            var segId = dataList.SegmentDetails.First(x => x.OriginAirportCode == s.BoardCity && x.DestinationAirportCode == s.OffCity && x.MarketingFlightNumber.PadLeft(4, '0') == s.FlightNumber).SegmentId;
                            var passenger = reservation.PassengerReservation.Passengers.Passenger.First(x => x.SpecialRequests.SpecialMealRequest != null && x.SpecialRequests.SpecialMealRequest.Contains(s));
                            var cust = dataList.CustomerDetails.First(x => passenger.nameId == $"{x.LastNameNumber.PadLeft(2, '0')}.{x.FirstNameNumber.PadLeft(2, '0')}");

                            if (passenger.SpecialRequests.SpecialMealRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.MealType == s.MealType && p.ActionCode == "KK"))
                            {
                                ois.Add(new()
                                {
                                    InstructionId = $"OI{instId++}",
                                    ItemCode = s.MealType,
                                    ItemType = OrderInstructions.ItemTypes.SSREnum,
                                    ItemText = $"FlightService/{cust.CustId}/{segId}/meal/{s.MealType}"
                                });
                            }
                            else
                                warnings?.Add(new()
                                {
                                    Code = "182",
                                    Details = s.MealType,
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                        }

                        if (ois.Count != 0)
                        {
                            orderItems.Add(new OrderItems()
                            {
                                OrderItemId = $"{orderId}-{orderItems.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                PriceDetails =
                                [
                                    new()
                            {
                                    Amount = new()
                                    {
                                        BaseCurrency = priceQuote?.Details?.FirstOrDefault()?.FareInfo?.TotalFare?.currencyCode,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }]
                            });
                        }
                    }
                }

                if (reservation.PassengerReservation.Passengers.Passenger.Any(x => x.SpecialRequests?.WheelchairRequest is not null))
                {
                    var ssrs = reservation.PassengerReservation.Passengers.Passenger.Where(p => p.SpecialRequests?.WheelchairRequest != null)?.SelectMany(x => x.SpecialRequests.WheelchairRequest.Where(s => s.ActionCode == "NN" || s.ActionCode == "HN"))?.GroupBy(x => x.WheelchairCode);

                    foreach (var ssr in ssrs)
                    {
                        foreach (var s in ssr)
                        {
                            var segId = dataList.SegmentDetails.First(x => x.OriginAirportCode == s.BoardCity && x.DestinationAirportCode == s.OffCity && x.MarketingFlightNumber.PadLeft(4, '0') == s.FlightNumber).SegmentId;
                            var passenger = reservation.PassengerReservation.Passengers.Passenger.First(x => x.SpecialRequests?.WheelchairRequest != null && x.SpecialRequests.WheelchairRequest.Contains(s));
                            var cust = dataList.CustomerDetails.First(x => passenger.nameId == $"{x.LastNameNumber.PadLeft(2, '0')}.{x.FirstNameNumber.PadLeft(2, '0')}");

                            if (passenger.SpecialRequests.WheelchairRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.WheelchairCode == s.WheelchairCode && p.ActionCode == "KK"))
                            {
                                ois.Add(new()
                                {
                                    InstructionId = $"OI{instId++}",
                                    ItemCode = s.WheelchairCode.ToString(),
                                    ItemType = OrderInstructions.ItemTypes.SSREnum,
                                    ItemText = $"FlightService/{cust.CustId}/{segId}/wheelChair/{s.WheelchairCode}"
                                });
                            }
                            else
                                warnings.Add(new()
                                {
                                    Code = "182",
                                    Details = s.WheelchairCode.ToString(),
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                        }

                        if (ois.Count != 0)
                        {
                            orderItems.Add(new OrderItems()
                            {
                                OrderItemId = $"{orderId}-{orderItems.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                PriceDetails =
                                [
                                    new()
                            {
                                    Amount = new()
                                    {
                                        BaseCurrency = priceQuote?.Details?.FirstOrDefault()?.FareInfo?.TotalFare?.currencyCode,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }]
                            });
                        }
                    }
                }

                if (reservation.PassengerReservation.Passengers.Passenger.Any(x => x.SpecialRequests?.GenericSpecialRequest is not null))
                {
                    var ssrs = reservation.PassengerReservation.Passengers.Passenger.Where(p => p.SpecialRequests?.GenericSpecialRequest != null)?.SelectMany(x => x.SpecialRequests.GenericSpecialRequest.Where(s => (s.ActionCode == "NN" || s.ActionCode == "HN") && s.Code != "OTHS" && s.Code != "INFT" && s.Code != "CHLD"))?.GroupBy(x => x.Code);

                    foreach (var ssr in ssrs)
                    {
                        foreach (var s in ssr)
                        {
                            var segId = dataList.SegmentDetails.FirstOrDefault(x => s.FullText.Contains($"{x.OriginAirportCode}{x.DestinationAirportCode}{x.MarketingFlightNumber.PadLeft(4, '0')}{Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}"))?.SegmentId ?? dataList.SegmentDetails.FirstOrDefault(x => s.FullText.Contains($"{x.OriginAirportCode}{x.DestinationAirportCode}{x.MarketingFlightNumber.PadLeft(4, '0')}{x.ClassOfService}{Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}"))?.SegmentId;
                            var passenger = reservation.PassengerReservation.Passengers.Passenger.First(x => x.SpecialRequests.GenericSpecialRequest != null && x.SpecialRequests.GenericSpecialRequest.Contains(s));
                            var cust = dataList.CustomerDetails.First(x => passenger.nameId == $"{x.LastNameNumber.PadLeft(2, '0')}.{x.FirstNameNumber.PadLeft(2, '0')}");

                            if (passenger.SpecialRequests.GenericSpecialRequest.Any(p => p.FullText == s.FullText.Replace("NN", "KK") && p.Code == s.Code && p.ActionCode == "KK"))
                            {
                                ois.Add(new()
                                {
                                    InstructionId = $"OI{instId++}",
                                    ItemCode = s.Code,
                                    ItemType = OrderInstructions.ItemTypes.SSREnum,
                                    ItemText = $"FlightService/{cust.CustId}/{segId}/general/{s.Code}"
                                });
                            }
                            else
                                warnings.Add(new()
                                {
                                    Code = "182",
                                    Details = s.Code,
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                        }

                        if (ois.Count != 0)
                        {
                            orderItems.Add(new OrderItems()
                            {
                                OrderItemId = $"{orderId}-{orderItems.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                PriceDetails =
                                [
                                    new()
                            {
                                    Amount = new()
                                    {
                                        BaseCurrency = priceQuote?.Details?.FirstOrDefault()?.FareInfo?.TotalFare?.currencyCode,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }]
                            });
                        }
                    }
                }

                dataList.OrderInstructions = ois;
            }

            return orderItems;
        }

        private static IEnumerable<Price> MapPriceDetails(string ptc, PriceQuoteInfoGetResponse priceQuote)
        {
            var fareInfo = priceQuote?.Details?.Where(x => x.passengerType == ptc)?.OrderByDescending(x => x.number)?.FirstOrDefault()?.FareInfo;

            if (fareInfo == null) return null;

            var prices = new List<Price>
            {
                new() {
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum,
                    Amount = new()
                    {
                        MonetaryAmount = fareInfo.TotalFare.Value,
                        BaseCurrency = fareInfo.TotalFare.currencyCode,
                        DecimalPrecisionCount = Convert.ToInt32(fareInfo.BaseFare.decimalPlace),
                    }
                }
            };
            // Add EquivalentFare if it exists
            if (fareInfo?.EquivalentFare != null)
            {
                prices.Add(new Price
                {
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum,
                    Amount = new()
                    {
                        MonetaryAmount = fareInfo.EquivalentFare.Value,
                        BaseCurrency = fareInfo.EquivalentFare.currencyCode,
                        DecimalPrecisionCount = Convert.ToInt32(fareInfo.EquivalentFare.decimalPlace),
                    },
                    BaseCurrencyAmount = new()
                    {
                        Amount = fareInfo?.BaseFare?.Value,
                        Currency = fareInfo?.BaseFare?.currencyCode,
                    },

                });
            }
            else
            {
                prices.Add(new Price
                {
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum,
                    Amount = new()
                    {
                        MonetaryAmount = fareInfo.BaseFare.Value,
                        BaseCurrency = fareInfo.BaseFare.currencyCode,
                        DecimalPrecisionCount = Convert.ToInt32(fareInfo.BaseFare.decimalPlace),
                    }
                });

            }
            if (fareInfo?.TotalTax != null && fareInfo?.TaxInfo?.Tax != null)
            {
                prices.Add(new Price
                {
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                    Amount = new()
                    {
                        MonetaryAmount = fareInfo.TotalTax.Value,
                        BaseCurrency = fareInfo.TotalTax.currencyCode,
                        DecimalPrecisionCount = Convert.ToInt32(fareInfo.BaseFare.decimalPlace),
                    },
                    TaxBreakup = fareInfo.TaxInfo.Tax.Select(x => new Tax
                    {
                        Code = x.code,
                        Amount = new()
                        {
                            MonetaryAmount = x.Amount.Value.ToString(),
                            BaseCurrency = x.Amount.currencyCode,
                            DecimalPrecisionCount = Convert.ToInt32(fareInfo.BaseFare.decimalPlace),
                        }
                    })
                });
            }

            return prices;

        }

        private static IEnumerable<OrderService> MapServices(string ptc, DataList dataList)
        {
            var serviceBeneficiariesRefIds = dataList.CustomerDetails.Where(x => x.Ptc == ptc).Select(x => x.CustId);

            return dataList.SegmentDetails.Select((value, index) =>
            {
                var serviceId = $"SV{++index}";

                return new OrderService()
                {
                    ServiceId = serviceId,
                    OriginDestinationRefIds = [value.OdRefId],
                    ServiceBeneficiaries = serviceBeneficiariesRefIds,
                    SegmentRefIds = [value.SegmentId],
                    PriceClassRefIds = [dataList.PriceClass.FirstOrDefault(x => x.FareBasisCode == value.FareBasis).Id],
                    ServiceDefinitionRefIds = [serviceId],
                    Product = new Product { ProductType = Product.ProductTypes.FlightEnum }
                };
            });
        }

        private static IEnumerable<AccountableDocumentRef> MapAccountableDocumentRefs(string ptc, ReservationPNRB reservation, DataList dataList)
        {
            if (reservation.PassengerReservation.TicketingInfo?.TicketDetails is null || reservation.PassengerReservation.Segments.Segment is null)
                return null;

            var ticketDetails = from ticket in reservation.PassengerReservation.TicketingInfo?.TicketDetails
                                from customer in dataList.CustomerDetails
                                where customer.Ptc == ptc && ticket.PassengerName.nameId.Split(".").First() == customer.LastNameNumber
                                && ticket.PassengerName.nameId.Split(".").Last() == customer.FirstNameNumber && ticket.Status != Constants.Application.TktInActive
                                select new { ticket, customer.CustId };



            return ticketDetails?.Select(ticket =>
            {
                return new AccountableDocumentRef()
                {
                    DocumentNumber = ticket.ticket.TicketNumber,
                    CustomerRefId = ticket.CustId,
                    DocumentType = AccountableDocumentRef.DocumentTypes.TEnum,
                    IssueDate = ticket.ticket.Timestamp.ToString(Constants.Application.DateOnlyFormat),
                    IssueTime = ticket.ticket.Timestamp.ToString(Constants.Application.TimeOnlyFormat)
                };
            });
        }

        private static IEnumerable<AccountableDocumentRef> MapAccountableDocumentRefs(IEnumerable<Customer> customers, ReservationPNRB reservation)
        {
            if (reservation.PassengerReservation.TicketingInfo?.TicketDetails is null || reservation.PassengerReservation.Segments.Segment is null || customers is null)
                return null;

            var ticketDetails = from ticket in reservation.PassengerReservation.TicketingInfo.TicketDetails
                                from customer in customers
                                where customer is not null
                                      && ticket.PassengerName.nameId.Split(".").First() == customer.LastNameNumber
                                      && ticket.PassengerName.nameId.Split(".").Last() == customer.FirstNameNumber
                                      && ticket.Status != Constants.Application.TktInActive
                                select new { ticket, customer.CustId };

            return ticketDetails.Select(ticket => new AccountableDocumentRef()
            {
                DocumentNumber = ticket.ticket.TicketNumber,
                CustomerRefId = ticket.CustId,
                DocumentType = AccountableDocumentRef.DocumentTypes.TEnum,
                IssueDate = ticket.ticket.Timestamp.ToString(Constants.Application.DateOnlyFormat),
                IssueTime = ticket.ticket.Timestamp.ToString(Constants.Application.TimeOnlyFormat)
            });
        }
    }

}
