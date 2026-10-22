using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.PriceQuote;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;
using GetReservationRS = VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.GetReservationRS;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper
{
    public static class SabreResponseToOrderDocumentMapper
    {
        public static OrderDocument Map(OrderSearchRequest request, GetReservationRS response, Result<EAPI.Domain.Entities.Common.Response<EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType>> getDeltaSeatServiceResponse, Result<EAPI.Domain.Entities.Common.Response<List<RetrieveResponseType>>> getDeltaSeatTicketResponse)
        {
            var orderId = request.SearchFilterCriteria.OrderReferenceCriteria.OrderID;
            var reservation = (ReservationPNRB)response.Item;
            var priceQuote = response.PriceQuote.Item != null ? (PriceQuoteInfoGetResponse)response.PriceQuote.Item : null;
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
                MapOriginDestinationAssociations(reservation.PassengerReservation.Segments.Segment, dataList, priceQuote, request.SearchFilterCriteria.IsCancelledAndRebooked, request.SearchFilterCriteria.HistoryFlag);

            dataList.CustomerDetails = MapCustomerDetails(reservation, request.SearchFilterCriteria.CustomerDetails);
            dataList.PriceClass = MapPriceClass(dataList.SegmentDetails);
            dataList.Journey = MapJourney(dataList.OriginDestinationAssociations, dataList.SegmentDetails);

            UpdateDataList(dataList);

            return new OrderDocument()
            {
                BookingReference = new()
                {
                    ExternalSystem = Constants.Application.ExternalSystem,
                    ExternalRecordLocator = ((SegmentTypePNRBSegmentAir)reservation.PassengerReservation.Segments?.Segment?.FirstOrDefault()?.Item)?.AirlineRefId.Split("*")?.LastOrDefault(),
                    RLOC = request.SearchFilterCriteria.BookingReferenceCriteria.BookingReference.RLOC,
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
                Parties = request.SearchFilterCriteria.Parties,
                TimeLimits = reservation.PassengerReservation.TicketingInfo.FutureTicketing?.Select(x => new TimeLimit { Limit = GetTTL(x) }),
                OrderNumber = new()
                {
                    OrderId = orderId,
                    OwnerAirline = Constants.Application.OwnerAirline
                },
                OrderRemarks = MapOrderRemarks(reservation.Remarks),
                OrderItems = MapOrderItems(orderId, reservation, priceQuote, dataList, getDeltaSeatServiceResponse, getDeltaSeatTicketResponse, false, warnings),
                OrderStatus = reservation.PassengerReservation.TicketingInfo.TicketDetails != null ? Constants.Application.OrderStatusConfirmed : string.Empty,
                Warnings = warnings
            };
        }

        private static string GetTTL(FutureTicketingPNRB timeLimit)
        {
            var hour = DateTime.ParseExact($"{timeLimit.Time}M", "hh00tt", CultureInfo.InvariantCulture).Hour;
            var date = DateTime.Parse(timeLimit.Date).AddHours(hour);
            return date.ToString(Constants.Application.DateTimeFormat);
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

        private static IEnumerable<PriceClass> MapPriceClass(IEnumerable<Segment> segments)
        {
            return segments.DistinctBy(x => x.FareBasis).Select((segment, index) => new PriceClass()
            {
                Id = $"{Constants.Application.PriceClassIdPrefix}{++index}",
                FareBasisCode = segment.FareBasis,
                CabinType = segment.CabinType,
            });
        }

        private static IEnumerable<OrderItems> MapOrderItems(string orderId, ReservationPNRB reservation, PriceQuoteInfoGetResponse priceQuote, DataList dataList, Result<EAPI.Domain.Entities.Common.Response<EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType>> getDeltaSeatServiceResponse, Result<EAPI.Domain.Entities.Common.Response<List<RetrieveResponseType>>> getDeltaSeatTicketResponse, [Optional] bool isOCN, [Optional] List<Warning> warnings, [Optional] bool isOrderHistory)
        {
            var orderItems = reservation.PassengerReservation.Passengers.Passenger.GroupBy(x => x.passengerType).Select((group, index) =>
             new OrderItems
             {
                 OrderItemId = $"{orderId}-{++index}",
                 ItemStatus = new()
                 {
                     Status = Constants.Application.OrderItemStatusCurrent
                 },
                 OwnerAirline = Constants.Application.OwnerAirline,
                 OCNTimestamp = isOCN ? DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) : null,
                 AccountableDocumentRefs = isOrderHistory ? MapOrderHistoryAccountableDocumentRefs(group.Key == Constants.Application.ChildPTCCodeCNN ? Constants.Application.ChildPTCCodeCHD : group.Key == Constants.Application.YoungAdultPTCCodeYTH ? Constants.Application.YoungAdultPTCCode : group.Key, reservation, dataList) : MapAccountableDocumentRefs(group.Key == Constants.Application.ChildPTCCodeCNN ? Constants.Application.ChildPTCCodeCHD : group.Key == Constants.Application.YoungAdultPTCCodeYTH ? Constants.Application.YoungAdultPTCCode : group.Key, reservation, dataList),
                 Services = MapServices(group.Key == Constants.Application.ChildPTCCodeCNN ? Constants.Application.ChildPTCCodeCHD : group.Key == Constants.Application.YoungAdultPTCCodeYTH ? Constants.Application.YoungAdultPTCCode : group.Key, dataList),
                 PriceDetails = MapPriceDetails(group.Key, priceQuote)
             }).ToList();

            var isSsrAvailable = reservation.PassengerReservation.Passengers.Passenger.Any(x => x.SpecialRequests?.SpecialMealRequest is not null || x.SpecialRequests?.WheelchairRequest is not null || x.SpecialRequests?.GenericSpecialRequest is not null);
            var seats = reservation.PassengerReservation.Passengers.Passenger.Any(x => x.Seats?.PreReservedSeats is not null);

            if (isSsrAvailable)
            {
                List<OrderInstructions> datalistOis = [];
                var instId = 1;

                if (reservation.PassengerReservation.Passengers.Passenger.Any(x => x.SpecialRequests?.SpecialMealRequest is not null))
                {
                    var ssrs = reservation.PassengerReservation.Passengers.Passenger.Where(p => p.SpecialRequests?.SpecialMealRequest != null)?.SelectMany(x => x.SpecialRequests.SpecialMealRequest.Where(s => s.ActionCode == "NN" || s.ActionCode == "HN"))?.GroupBy(x => x.MealType);

                    foreach (var ssr in ssrs)
                    {
                        List<OrderInstructions> ois = [];

                        foreach (var s in ssr)
                        {
                            var segId = dataList.SegmentDetails.First(x => x.OriginAirportCode == s.BoardCity && x.DestinationAirportCode == s.OffCity && x.MarketingFlightNumber.PadLeft(4, '0') == s.FlightNumber).SegmentId;
                            var passenger = reservation.PassengerReservation.Passengers.Passenger.First(x => x.SpecialRequests.SpecialMealRequest != null && x.SpecialRequests.SpecialMealRequest.Contains(s));
                            var cust = dataList.CustomerDetails.First(x => passenger.nameId == $"{x.LastNameNumber.PadLeft(2, '0')}.{x.FirstNameNumber.PadLeft(2, '0')}");

                            if (passenger.SpecialRequests.SpecialMealRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.MealType == s.MealType && p.type == RequestTypePNRB.A && p.ActionCode == "KK"))
                            {
                                ois.Add(new()
                                {
                                    InstructionId = $"OI{instId++}",
                                    ItemCode = s.MealType,
                                    ItemType = OrderInstructions.ItemTypes.SSREnum,
                                    ItemText = $"FlightService/{cust.CustId}/{segId}/meal/{s.MealType}"
                                });
                            }
                            else if (passenger.SpecialRequests.SpecialMealRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.MealType == s.MealType && p.type == RequestTypePNRB.A && (p.ActionCode == "NO" || p.ActionCode == "UN")))
                                warnings?.Add(new()
                                {
                                    Code = "186",
                                    Details = s.MealType,
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
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
                            datalistOis.AddRange(ois);

                            orderItems.Add(new OrderItems()
                            {
                                OrderItemId = $"{orderId}-{orderItems.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                OCNTimestamp = isOCN ? DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) : null,
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
                        List<OrderInstructions> ois = [];

                        foreach (var s in ssr)
                        {
                            var segId = dataList.SegmentDetails.First(x => x.OriginAirportCode == s.BoardCity && x.DestinationAirportCode == s.OffCity && x.MarketingFlightNumber.PadLeft(4, '0') == s.FlightNumber).SegmentId;
                            var passenger = reservation.PassengerReservation.Passengers.Passenger.First(x => x.SpecialRequests?.WheelchairRequest != null && x.SpecialRequests.WheelchairRequest.Contains(s));
                            var cust = dataList.CustomerDetails.First(x => passenger.nameId == $"{x.LastNameNumber.PadLeft(2, '0')}.{x.FirstNameNumber.PadLeft(2, '0')}");

                            if (passenger.SpecialRequests.WheelchairRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.WheelchairCode == s.WheelchairCode && p.type == RequestTypePNRB.A && p.ActionCode == "KK"))
                            {
                                ois.Add(new()
                                {
                                    InstructionId = $"OI{instId++}",
                                    ItemCode = s.WheelchairCode.ToString(),
                                    ItemType = OrderInstructions.ItemTypes.SSREnum,
                                    ItemText = $"FlightService/{cust.CustId}/{segId}/wheelChair/{s.WheelchairCode}"
                                });
                            }
                            else if (passenger.SpecialRequests.WheelchairRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.WheelchairCode == s.WheelchairCode && p.type == RequestTypePNRB.A && (p.ActionCode == "NO" || p.ActionCode == "UN")))
                                warnings?.Add(new()
                                {
                                    Code = "186",
                                    Details = s.WheelchairCode.ToString(),
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                            else
                                warnings?.Add(new()
                                {
                                    Code = "182",
                                    Details = s.WheelchairCode.ToString(),
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                        }

                        if (ois.Count != 0)
                        {
                            datalistOis.AddRange(ois);

                            orderItems.Add(new OrderItems()
                            {
                                OrderItemId = $"{orderId}-{orderItems.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                OCNTimestamp = isOCN ? DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) : null,
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
                        List<OrderInstructions> ois = [];

                        foreach (var s in ssr)
                        {
                            var segId = dataList.SegmentDetails.FirstOrDefault(x => s.FullText.Contains($"{x.OriginAirportCode}{x.DestinationAirportCode}{x.MarketingFlightNumber.PadLeft(4, '0')}{Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}"))?.SegmentId ?? dataList.SegmentDetails.FirstOrDefault(x => s.FullText.Contains($"{x.OriginAirportCode}{x.DestinationAirportCode}{x.MarketingFlightNumber.PadLeft(4, '0')}{x.ClassOfService}{Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}"))?.SegmentId;
                            var passenger = reservation.PassengerReservation.Passengers.Passenger.First(x => x.SpecialRequests.GenericSpecialRequest != null && x.SpecialRequests.GenericSpecialRequest.Contains(s));
                            var cust = dataList.CustomerDetails.First(x => passenger.nameId == $"{x.LastNameNumber.PadLeft(2, '0')}.{x.FirstNameNumber.PadLeft(2, '0')}");

                            if (passenger.SpecialRequests.GenericSpecialRequest.Any(p => p.FullText == s.FullText.Replace("NN", "KK") && p.Code == s.Code && p.type == RequestTypePNRB.A && p.ActionCode == "KK"))
                            {
                                ois.Add(new()
                                {
                                    InstructionId = $"OI{instId++}",
                                    ItemCode = s.Code,
                                    ItemType = OrderInstructions.ItemTypes.SSREnum,
                                    ItemText = $"FlightService/{cust.CustId}/{segId}/general/{s.Code}"
                                });
                            }
                            else if (passenger.SpecialRequests.GenericSpecialRequest.Any(p => p.FullText == s.FullText.Replace("NN", "KK") && p.Code == s.Code && p.type == RequestTypePNRB.A && (p.ActionCode == "NO" || p.ActionCode == "UN")))
                                warnings?.Add(new()
                                {
                                    Code = "186",
                                    Details = s.Code,
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                            else
                                warnings?.Add(new()
                                {
                                    Code = "182",
                                    Details = s.Code,
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                        }

                        if (ois.Count != 0)
                        {
                            datalistOis.AddRange(ois);

                            orderItems.Add(new OrderItems()
                            {
                                OrderItemId = $"{orderId}-{orderItems.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                OCNTimestamp = isOCN ? DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) : null,
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

                dataList.OrderInstructions = datalistOis;
            }


            if (seats)
            {
                var seatOrderItems = from ptcGroup in reservation.PassengerReservation.Passengers.Passenger.Where(x => x.Seats.PreReservedSeats is not null).GroupBy(x => x.passengerType)
                                     from passenger in ptcGroup
                                     from seat in passenger.Seats.PreReservedSeats
                                     select new { ptcGroup.Key, seat };

                var seatIndex = orderItems.Count;

                orderItems.AddRange(seatOrderItems.Select(seat => new OrderItems
                {
                    OrderItemId = $"{orderId}-{++seatIndex}",
                    ItemStatus = new()
                    {
                        Status = Constants.Application.OrderItemStatusCurrent
                    },
                    OwnerAirline = Constants.Application.OwnerAirline,
                    OCNTimestamp = isOCN ? DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) : null,
                    AccountableDocumentRefs = MapSeatAccountableDocumentRefs(seat.Key == Constants.Application.ChildPTCCodeCNN ? Constants.Application.ChildPTCCodeCHD : seat.Key == Constants.Application.YoungAdultPTCCodeYTH ? Constants.Application.YoungAdultPTCCode : seat.Key, reservation, dataList, null, seat.seat, getDeltaSeatServiceResponse, getDeltaSeatTicketResponse),
                    Services = MapSeatServices(seat.Key == Constants.Application.ChildPTCCodeCNN ? Constants.Application.ChildPTCCodeCHD : seat.Key == Constants.Application.YoungAdultPTCCodeYTH ? Constants.Application.YoungAdultPTCCode : seat.Key, dataList, seat.seat),
                    PriceDetails = MapPriceSeatDetails(seat.Key == Constants.Application.ChildPTCCodeCNN ? Constants.Application.ChildPTCCodeCHD : seat.Key == Constants.Application.YoungAdultPTCCodeYTH ? Constants.Application.YoungAdultPTCCode : seat.Key, reservation, dataList, seat.seat, getDeltaSeatServiceResponse, getDeltaSeatTicketResponse)
                }));
            }

            return orderItems;
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

        private static IEnumerable<AccountableDocumentRef> MapOrderHistoryAccountableDocumentRefs(string ptc, ReservationPNRB reservation, DataList dataList)
        {
            if (reservation.PassengerReservation.TicketingInfo?.TicketDetails is null || reservation.PassengerReservation.Segments?.Segment is null)
                return null;

            var latestTimestamp = reservation.PassengerReservation.TicketingInfo.TicketDetails.Select(x => x.Timestamp).Distinct().OrderByDescending(x => x).FirstOrDefault();
            var latestTicketdetails = reservation.PassengerReservation.TicketingInfo.TicketDetails.Where(x => x.Timestamp == latestTimestamp);

            var ticketDetails = from ticket in latestTicketdetails
                                let nameId = ticket.PassengerName.nameId.Split('.')
                                from customer in dataList.CustomerDetails
                                where customer.Ptc == ptc && nameId.First() == customer.LastNameNumber
                                && nameId.Last() == customer.FirstNameNumber && ticket.Status != Constants.Application.TktInActive
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

        private static IEnumerable<AccountableDocumentRef> MapSeatAccountableDocumentRefs(string ptc, ReservationPNRB reservation, DataList dataList, object value, PreReservedSeatPNRB seat, Result<EAPI.Domain.Entities.Common.Response<EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType>> getDeltaSeatServiceResponse, Result<EAPI.Domain.Entities.Common.Response<List<RetrieveResponseType>>> getDeltaSeatTicketResponse)
        {
            if (reservation.PassengerReservation.TicketingInfo?.TicketDetails is null)
                return null;

            var occupiedSeatCustname = from customer in dataList.CustomerDetails
                                       where customer.Ptc == ptc && seat?.NameNumber.Split(".").First() == customer.LastNameNumber && seat?.NameNumber.Split(".").Last() == customer.FirstNameNumber
                                       select customer.LastName + "/" + customer.FirstName;

            var accDoc = new List<AccountableDocumentRef>();

            var selectedTicket = getDeltaSeatTicketResponse.Value.Value.Where(x => x.TicketRequestResponse.First().TicketResponse.First().PassengerInformation.PassengerName.Equals(occupiedSeatCustname.First().Replace("/", "")) && x.TicketRequestResponse.First().TicketResponse.First().TicketCoupon.Any(x => x.TickeCouponOriginatingAirport == seat.BoardPoint && x.TicketCouponDestinationAirport == seat.OffPoint));

            var selectedRemark = FindSeatEfeeRemark(getDeltaSeatServiceResponse.Value.Value.TeardownContextResponse.Pnr.Remark,occupiedSeatCustname.First(),seat);

            if (selectedRemark is not null)
            {
                accDoc.Add(new AccountableDocumentRef
                {
                    DocumentNumber = selectedRemark.FreeformText.Split("/")[1],
                    CustomerRefId = dataList.CustomerDetails.First(cust => cust.LastName + cust.FirstName == occupiedSeatCustname.First().Replace("/", "")).CustId,
                    DocumentType = AccountableDocumentRef.DocumentTypes.YEnum,
                    IssueDate = DateTime.ParseExact(selectedTicket.First().TicketRequestResponse.First().TicketResponse.First().TicketInformation.TicketCreateDate, "ddMMMyy", CultureInfo.InvariantCulture).ToString(Constants.Application.DateOnlyFormat),
                    FareCalculation = selectedTicket.First().TicketRequestResponse.First().TicketResponse.First().EtRemarks.First().FreeFormText,
                    Coupons = GetSeatCouponInfo(selectedTicket, dataList, selectedRemark, seat)
                });
            }
            else
            {
                accDoc.Add(new AccountableDocumentRef
                {
                    CustomerRefId = dataList.CustomerDetails.First(cust => cust.LastName + cust.FirstName == occupiedSeatCustname.First().Replace("/", "")).CustId,
                    DocumentType = AccountableDocumentRef.DocumentTypes.YEnum,
                    IssueDate = DateTime.ParseExact(selectedTicket.First().TicketRequestResponse.First().TicketResponse.First().TicketInformation.TicketCreateDate, "ddMMMyy", CultureInfo.InvariantCulture).ToString(Constants.Application.DateOnlyFormat),
                    FareCalculation = selectedTicket.First().TicketRequestResponse.First().TicketResponse.First().EtRemarks.First().FreeFormText,
                    Coupons = GetSeatCouponInfo(selectedTicket, dataList, null, seat)
                });

            }

            return accDoc;
        }

        private static EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.Remark FindSeatEfeeRemark(
            IEnumerable<EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.Remark> remarks,
            string customerName,
            PreReservedSeatPNRB seat)
        {
            var seatRoute = seat.BoardPoint + seat.OffPoint;
            var efeeRemarks = remarks?
                .Where(remark => remark.FreeformText?.Contains("EFEE") == true && remark.FreeformText.Split("/").Length > 3)
                .ToList();

            return efeeRemarks?.FirstOrDefault(remark => remark.FreeformText.Contains(customerName) && remark.FreeformText.Contains(seatRoute))
                ?? efeeRemarks?.FirstOrDefault(remark => remark.FreeformText.Contains(seatRoute));
        }

        private static IEnumerable<Coupon> GetSeatCouponInfo(IEnumerable<RetrieveResponseType> selectedTicket, DataList dataList, EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.Remark remarks, PreReservedSeatPNRB seat)
        {
            return
            [ new Coupon()
            {
                CouponNumber = selectedTicket.First().TicketRequestResponse.First().TicketResponse.First().TicketCoupon.First().CouponSequenceNumber,
                TicketNumber = remarks is null ? selectedTicket.First().TicketRequestResponse.First().TicketRequest.ElectronicDocumentNumber : remarks.FreeformText.Split("/")[1],
                CouponStatus = selectedTicket.First().TicketRequestResponse.First().TicketResponse.First().TicketCoupon.First().CouponStatus,
                SegmentRefId = dataList.SegmentDetails.Where(x=> x.OriginAirportCode == seat.BoardPoint && x.DestinationAirportCode == seat.OffPoint).First().SegmentId,
                FareBasis = selectedTicket.First().TicketRequestResponse.First().TicketResponse.First().TicketCoupon.First().FarebasisCode,
            }
            ];
        }

        private static IEnumerable<Price> MapPriceSeatDetails(string ptc, ReservationPNRB reservation, DataList dataList, PreReservedSeatPNRB seat, Result<EAPI.Domain.Entities.Common.Response<EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType>> getDeltaSeatServiceResponse, Result<EAPI.Domain.Entities.Common.Response<List<RetrieveResponseType>>> getDeltaSeatTicketResponse)
        {
            var occupiedSeatCustname = from customer in dataList.CustomerDetails
                                       where customer.Ptc == ptc && seat?.NameNumber.Split(".").First() == customer.LastNameNumber && seat?.NameNumber.Split(".").Last() == customer.FirstNameNumber
                                       select customer.LastName + "/" + customer.FirstName;

            var prices = new List<Price>();

            var selectedRemark = FindSeatEfeeRemark(
                getDeltaSeatServiceResponse.Value.Value.TeardownContextResponse.Pnr.Remark,
                occupiedSeatCustname.First(),
                seat);

            var ticketInfo = getDeltaSeatTicketResponse.Value.Value.First().TicketRequestResponse.First().TicketResponse.First().TicketInformation;

            if (selectedRemark is not null)
            {
                prices.Add(new Price
                {
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum,
                    Amount = new()
                    {
                        MonetaryAmount = selectedRemark.FreeformText.Split("/")[2].Remove(0, 3),
                        BaseCurrency = selectedRemark.FreeformText.Split("/")[2].Replace(selectedRemark.FreeformText.Split("/")[2].Remove(0, 3), ""),
                        DecimalPrecisionCount = Convert.ToInt32(selectedRemark.FreeformText.Split("/")[2].Remove(0, 3).Split(".")[1].Length),
                    }
                });
            }
            else
            {
                prices.Add(new Price
                {
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum,
                    Amount = new()
                    {
                        MonetaryAmount = Constants.Application.FreeSeatAmount,
                        BaseCurrency = string.IsNullOrEmpty(ticketInfo.CurrencyCodeOfEquivalentFare) ? ticketInfo.CurrencyCodeOfBaseFare : ticketInfo.CurrencyCodeOfEquivalentFare
                    }
                });
            }

            return prices;
        }

        private static IEnumerable<OrderService> MapServices(string ptc, DataList dataList)
        {
            var serviceBeneficiariesRefIds = dataList.CustomerDetails.Where(x => x.Ptc == ptc).Select(x => x.CustId);

            return dataList.SegmentDetails.Select((value, index) =>
            {
                var serviceId = $"{Constants.Application.ServiceIdPrefix}{++index}";
                var orderInstructionId = $"{Constants.Application.OrderInstructionIdPrefix}{index}";

                return new OrderService()
                {
                    ServiceId = serviceId,
                    OriginDestinationRefIds = [value.OdRefId],
                    ServiceBeneficiaries = serviceBeneficiariesRefIds,
                    SegmentRefIds = [value.SegmentId],
                    PriceClassRefIds = value.FareBasis != null ? [dataList.PriceClass.FirstOrDefault(x => x.FareBasisCode == value.FareBasis)?.Id] : null,
                    ServiceDefinitionRefIds = [serviceId],
                    Product = new Product { ProductType = Product.ProductTypes.FlightEnum }
                };
            });
        }

        private static IEnumerable<OrderService> MapSeatServices(string ptc, DataList dataList, PreReservedSeatPNRB seat)
        {
            var serviceBeneficiariesRefIds = dataList.CustomerDetails.Where(x => x.Ptc == ptc).Select(x => x.CustId);

            var seatCustId = from customer in dataList.CustomerDetails
                             where customer.Ptc == ptc && seat?.NameNumber.Split(".").First() == customer.LastNameNumber && seat?.NameNumber.Split(".").Last() == customer.FirstNameNumber
                             select customer.CustId;

            var datalistInfo = dataList.SegmentDetails.Where(x => x.DestinationAirportCode == seat.OffPoint && x.OriginAirportCode == seat.BoardPoint);
            int index = 0;

            var serviceId = $"{Constants.Application.ServiceIdPrefix}{++index}";
            return [ new OrderService()
            {
                ServiceId = serviceId,
                OriginDestinationRefIds = [datalistInfo.First().OdRefId],
                ServiceBeneficiaries = seat is null ? serviceBeneficiariesRefIds : seatCustId,
                SegmentRefIds = [datalistInfo.First().SegmentId],
                PriceClassRefIds = [dataList.PriceClass.FirstOrDefault(x => x.FareBasisCode == datalistInfo.First().FareBasis).Id],
                Product = seat is null ? new Product { ProductType = Product.ProductTypes.FlightEnum, } : new Product { ProductType = Product.ProductTypes.SeatEnum, },
                CurrentSeatAssignment =  seat?.SeatNumber,
            }];
        }

        private static IEnumerable<VAA.EAPI.Domain.Virgin.Models.Common.Remark> MapOrderRemarks(RemarksPNRB remarks)
        {
            return remarks?.Remark.Select(remark =>
            new VAA.EAPI.Domain.Virgin.Models.Common.Remark()
            {
                Text = remark?.RemarkLines?.RemarkLine?.FirstOrDefault()?.Text
            });
        }

        private static void MapOriginDestinationAssociations(SegmentTypePNRBSegment[] segments, DataList dataList, PriceQuoteInfoGetResponse priceQuote, [Optional] bool isCancelledAndRebooked, [Optional] bool historyFlag)
        {
            var originDestinationAssociationsList = new List<OriginDestinationAssociation>();
            string departureAirport = "";
            string arrivalAirport = "";
            string departureDateTime = "";
            var segmentDetailsList = dataList.SegmentDetails.ToList();

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Item is Arunk || isCancelledAndRebooked && (segments[i].Item as SegmentTypePNRBSegmentAir).ActionCode.Is("UN"))
                    continue;

                var segment = (SegmentTypePNRBSegmentAir)segments[i].Item;
                var index = originDestinationAssociationsList.Count;
                var odRefId = $"{Constants.Application.OriginDestinationIdPrefix}{index + 1}";
                var ond = new OriginDestinationAssociation();
                var connectionDuration = ((!segment.outboundConnection && !segment.inboundConnection) || (segment.outboundConnection && !segment.inboundConnection)) ? null : DateTime.Parse(segment.DepartureDateTime).Subtract(DateTime.Parse(((SegmentTypePNRBSegmentAir)segments[i - 1].Item).ArrivalDateTime)).TotalMinutes.ToString();

                MapSegmentDetails(segment, segments[i].Product, odRefId, segmentDetailsList, priceQuote, connectionDuration, historyFlag);

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

        private static IEnumerable<Customer> MapCustomerDetails(ReservationPNRB reservation, IEnumerable<Customer> customers)
        {
            return reservation.PassengerReservation.Passengers.Passenger.GroupBy(x => x.passengerType).SelectMany(group =>
            {
                return group.Select((passenger, index) =>
                {
                    var custDetails = customers.First(c => c.FirstName.Equals(passenger.FirstName, StringComparison.OrdinalIgnoreCase) && c.LastName.Equals(passenger.LastName, StringComparison.OrdinalIgnoreCase));
                    string infantCustId = null;
                    string associatedAdult = null;
                    PassengerPNRB associatedAdultPax = null;

                    if (passenger.withInfant || passenger.passengerType == Constants.Application.InfantPTCCode)
                    {
                        var infantName = passenger.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(x => x.Code == "INFT")?.FreeText?.Split("/");
                        infantCustId = passenger.withInfant ? customers.First(c => c.FirstName.Equals(infantName[2], StringComparison.OrdinalIgnoreCase) && c.LastName.Equals(infantName[1], StringComparison.OrdinalIgnoreCase)).CustId : null;
                        associatedAdultPax = passenger.passengerType == Constants.Application.InfantPTCCode ? reservation.PassengerReservation.Passengers.Passenger.FirstOrDefault(p => p.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(s => s.Code == "INFT")?.FreeText?.Split("/")?[2] == passenger.FirstName && p.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(s => s.Code == "INFT")?.FreeText?.Split("/")?[1] == passenger.LastName) : null;
                        associatedAdult = associatedAdultPax == null ? null : customers.FirstOrDefault(c => c.FirstName.Equals(associatedAdultPax.FirstName, StringComparison.OrdinalIgnoreCase) && c.LastName.Equals(associatedAdultPax.LastName, StringComparison.OrdinalIgnoreCase))?.CustId;
                    }

                    return new Customer()
                    {
                        CustId = custDetails.CustId,
                        LinkedInfantId = passenger.withInfant ? infantCustId : null,
                        AssociatedWithAdult = passenger.passengerType == Constants.Application.InfantPTCCode ? associatedAdult : null,
                        Ptc = passenger.passengerType.Replace(Constants.Application.ChildPTCCodeCNN, Constants.Application.ChildPTCCodeCHD).Replace(Constants.Application.YoungAdultPTCCodeYTH, Constants.Application.YoungAdultPTCCode),
                        LastName = passenger.LastName,
                        LastNameNumber = passenger.nameId.Split(".").First(),
                        FirstName = passenger.FirstName,
                        FirstNameNumber = passenger.nameId.Split(".").Last(),
                        Gender = passenger.passengerType == Constants.Application.InfantPTCCode ? reservation.PassengerReservation.Passengers.Passenger.FirstOrDefault(p => (bool)p.SpecialRequests?.APISRequest?.Any(a => a?.DOCSEntry?.Forename == passenger.FirstName && a?.DOCSEntry?.Surname == passenger.LastName && a?.DOCSEntry?.Gender != null))?.SpecialRequests?.APISRequest?.FirstOrDefault(a => a.DOCSEntry?.Forename == passenger.FirstName && a?.DOCSEntry?.Surname == passenger.LastName && a?.DOCSEntry?.Gender != null)?.DOCSEntry?.Gender.Replace("I", "") : passenger.SpecialRequests?.APISRequest?.First(x => x.DOCSEntry != null).DOCSEntry.Gender,
                        DateOfBirth = passenger.passengerType == Constants.Application.InfantPTCCode ? reservation.PassengerReservation.Passengers.Passenger.FirstOrDefault(p => (bool)p.SpecialRequests?.APISRequest?.Any(a => a?.DOCSEntry?.Forename == passenger.FirstName && a?.DOCSEntry?.Surname == passenger.LastName && a?.DOCSEntry?.Gender != null))?.SpecialRequests?.APISRequest?.FirstOrDefault(a => a.DOCSEntry?.Forename == passenger.FirstName && a?.DOCSEntry?.Surname == passenger.LastName && a?.DOCSEntry?.Gender != null)?.DOCSEntry?.DateOfBirth!.ToString(Constants.Application.DateOnlyFormat) : passenger.SpecialRequests?.APISRequest?.First(x => x.DOCSEntry != null).DOCSEntry?.DateOfBirth!.ToString(Constants.Application.DateOnlyFormat),
                        Contacts = GetPnrPassengerContacts(reservation, passenger, custDetails.Contacts),
                        Employer = MapGstDetails(reservation, passenger.SpecialRequests?.GenericSpecialRequest),
                        TravelDocuments = MapTravelDocuments(passenger.passengerType == Constants.Application.InfantPTCCode ? associatedAdultPax : passenger, customers, passenger.passengerType == Constants.Application.InfantPTCCode, passenger.FirstName, passenger.LastName),
                        Addresses = MapAddress(passenger.SpecialRequests?.APISRequest?.FirstOrDefault()?.DOCAEntry),
                        LoyaltyMemberDetails = passenger.FrequentFlyer != null && passenger.FrequentFlyer.Any() ? new LoyaltyMemberDetails()
                        {
                            MemberNumber = passenger.FrequentFlyer.FirstOrDefault()?.Number,
                            ProgramCode = passenger.FrequentFlyer.FirstOrDefault()?.SupplierCode
                        } : null
                    };
                });
            });
        }

        private static Contacts GetPnrPassengerContacts(ReservationPNRB reservation, PassengerPNRB passenger, Contacts originalContacts)
        {
            if (passenger.passengerType == Constants.Application.InfantPTCCode)
            {
                return null;
            }

            var passengerServiceRequests = reservation?.OpenReservationElements?.OpenReservationElement?
                .Where(element => element.NameAssociation?.Any(nameAssociation =>
                    string.Equals(nameAssociation.NameRefNumber, passenger.nameId, StringComparison.OrdinalIgnoreCase)) == true)
                .Select(element => element.Item)
                .OfType<ServiceRequestType>()
                .ToArray();

            var email = passengerServiceRequests?
                .Where(serviceRequest => serviceRequest.code == "CTCE")
                .Select(serviceRequest => (serviceRequest.Item as ServiceRequestTypePassengerContactEmail)?.Email)
                .FirstOrDefault(emailAddress => !string.IsNullOrWhiteSpace(emailAddress));
            var phone = passengerServiceRequests?
                .Where(serviceRequest => serviceRequest.code == "CTCM")
                .Select(serviceRequest => (serviceRequest.Item as ServiceRequestTypePassengerContactMobilePhone)?.PhoneNumber)
                .FirstOrDefault(phoneNumber => !string.IsNullOrWhiteSpace(phoneNumber));

            var countryDialingCode = originalContacts?.CountryDailingCode;
            var originalPhone = originalContacts?.Phone?.Replace("-", string.Empty);
            if (string.IsNullOrWhiteSpace(countryDialingCode)
                || string.IsNullOrWhiteSpace(originalPhone)
                || !string.Equals(phone, $"{countryDialingCode}{originalPhone}", StringComparison.Ordinal))
            {
                countryDialingCode = null;
            }
            else
            {
                phone = phone[countryDialingCode.Length..];
            }

            var hasContactDetails = !string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(phone);
            var contactRefused = !hasContactDetails && passengerServiceRequests?.Any(serviceRequest => serviceRequest.code == "CTCR") == true;
            return !hasContactDetails && !contactRefused
                ? null
                : new Contacts
                {
                    Email = email,
                    Phone = phone,
                    CountryDailingCode = countryDialingCode,
                    ContactRefusedInd = contactRefused
                };
        }

        private static Employer MapGstDetails(ReservationPNRB reservation, GenericSpecialRequestPNRB[] genericSpecialRequest)
        {
            if (genericSpecialRequest != null && genericSpecialRequest.Any(x => x.Code == Constants.Application.GSTA || x.Code == Constants.Application.GSTE || x.Code == Constants.Application.GSTN || x.Code == Constants.Application.GSTP))
            {
                var employer = new Employer();
                List<SalesTaxRegistration> gstDetails = [];

                if (genericSpecialRequest.Any(x => x.Code == Constants.Application.GSTN))
                {
                    employer.Name = reservation.PassengerReservation.Passengers.Passenger.FirstOrDefault().SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(x => x?.Code == "GSTN").FreeText.Split("/").Last();
                    employer.TaxDeductedAtSourceCapabilityInd = true;
                }

                foreach (var psg in reservation.PassengerReservation.Passengers.Passenger)
                {
                    if (psg.SpecialRequests?.GenericSpecialRequest is not null)
                    {
                        var gstInfo = psg.SpecialRequests?.GenericSpecialRequest;
                        var gSTA = gstInfo?.FirstOrDefault(x => x.Code == Constants.Application.GSTA);
                        var gSTE = gstInfo?.FirstOrDefault(x => x.Code == Constants.Application.GSTE);
                        var gSTP = gstInfo?.FirstOrDefault(x => x.Code == Constants.Application.GSTP);
                        var gSTN = gstInfo?.FirstOrDefault(x => x.Code == Constants.Application.GSTN);

                        gstDetails.Add(new SalesTaxRegistration()
                        {
                            Contact = new()
                            {
                                CountryDailingCode = gSTA?.FreeText.Split("/")[1],
                                Countrycode = gSTA?.FreeText.Split("/")[1],
                                Email = gSTE != null ? gSTE?.FreeText.Split("/")[2] + "@" + gSTE?.FreeText.Split("/").Last() : null,
                                Phone = gSTP?.FreeText.Split("/").Last(),
                            },

                            Address = new()
                            {
                                PostalCode = gSTA?.FreeText.Split("//").Last(),
                                AddressLine1 = gSTA?.FullText.Split("/")[2],
                                AddressLine2 = gSTA?.FullText.Split("/")[3],
                            }
                        });
                    }
                }
                ;

                employer.SalesTaxRegistration = gstDetails.FirstOrDefault();

                return employer;
            }

            return null;
        }

        private static Address MapAddress(DOCAEntryPNRB dOCAEntry)
        {
            return dOCAEntry is null ? null : new()
            {
                AddressLine1 = dOCAEntry.Street,
                AddressType = dOCAEntry.AddressType.ToString(),
                City = dOCAEntry.City,
                Country = dOCAEntry.Country,
                PostalCode = dOCAEntry.PostCode,
                Region = dOCAEntry.StateProvince,
            };
        }

        private static List<TravelDocument> MapTravelDocuments(PassengerPNRB passenger, IEnumerable<Customer> customers, bool infant, string firstName, string lastName)
        {
            if (passenger?.SpecialRequests?.APISRequest == null) return null;

            var travelDocuments = new List<TravelDocument>();

            for (int i = 0; i < passenger.SpecialRequests.APISRequest.Length; i++)
            {
                var apisReq = passenger.SpecialRequests.APISRequest[i];

                if (apisReq.DOCSEntry != null && (apisReq.DOCSEntry?.DocumentType == Constants.Application.PassportDocumentType || apisReq.DOCSEntry?.DocumentType == Constants.Application.GreenCardDocumentType) && apisReq.DOCSEntry.Forename == firstName && apisReq.DOCSEntry.Surname == lastName)
                {
                    var issueDate = customers.Where(c => c.FirstName.Is(apisReq.DOCSEntry.Forename) && c.LastName.Is(apisReq.DOCSEntry.Surname) && c.TravelDocuments != null).SelectMany(c => c.TravelDocuments).FirstOrDefault(x => x.DocumentNumber == apisReq.DOCSEntry.DocumentNumber)?.IssueDate;

                    travelDocuments.Add(new()
                    {
                        DocumentType = apisReq.DOCSEntry.DocumentType == Constants.Application.PassportDocumentType ? Constants.Application.NDCPassportDocumentType : apisReq.DOCSEntry.DocumentType == Constants.Application.GreenCardDocumentType ? Constants.Application.NDCGreenCardDocumentType : null,
                        DocumentNumber = apisReq.DOCSEntry.DocumentNumber,
                        CountryOfIssue = apisReq.DOCSEntry.CountryOfIssue,
                        DateOfBirth = apisReq.DOCSEntry.DateOfBirth.ToString(Constants.Application.DateOnlyFormat),
                        ExpirationDate = apisReq.DOCSEntry.DocumentExpirationDate.ToString(Constants.Application.DateOnlyFormat),
                        Gender = apisReq.DOCSEntry.Gender,
                        Nationality = apisReq.DOCSEntry.DocumentNationalityCountry,
                        CountryOfResidence = apisReq.DOCSEntry.DocumentNationalityCountry,
                        PersonName = new()
                        {
                            FirstName = apisReq.DOCSEntry.Forename,
                            LastName = apisReq.DOCSEntry.Surname,
                        },
                        IssueDate = issueDate != null ? Convert.ToDateTime(issueDate).ToString(Constants.Application.DateOnlyFormat) : null
                    });
                }

                if (apisReq.DOCOEntry != null && apisReq.DOCOEntry.FreeText.Contains("/V/") && (infant ? apisReq.DOCOEntry.FreeText.Contains("/I/") : !apisReq.DOCOEntry.FreeText.Contains("/I/")))
                {
                    var issueDate = customers.Where(c => c.TravelDocuments != null)?.SelectMany(c => c.TravelDocuments)?.FirstOrDefault(x => x.DocumentNumber == apisReq.DOCOEntry.FreeText.Split("/")[3])?.IssueDate;

                    travelDocuments.Add(new()
                    {
                        DocumentType = Constants.Application.NDCVisaDocumentType,
                        DocumentNumber = apisReq.DOCOEntry.FreeText.Split("/")[3],
                        Nationality = apisReq.DOCOEntry.FreeText.Split("/")[1],
                        CountryOfIssue = apisReq.DOCOEntry.FreeText.Split("/")[1],
                        IssueDate = apisReq.DOCOEntry.VisaIssueDateSpecified ? apisReq.DOCOEntry.VisaIssueDate.ToString(Constants.Application.DateOnlyFormat) : issueDate != null ? Convert.ToDateTime(issueDate).ToString(Constants.Application.DateOnlyFormat) : null,
                        RegionOfIssuance = apisReq.DOCOEntry.FreeText.Split("/")[1],
                        CountryOfResidence = apisReq.DOCOEntry.FreeText.Split("/")[1],
                        ExpirationDate = Convert.ToDateTime(infant ? apisReq.DOCOEntry.FreeText.Split("/").Last() : apisReq.DOCOEntry.FreeText.Split("//").Last()).ToString(Constants.Application.DateOnlyFormat)
                    });
                }

                if (apisReq.DOCOEntry != null && apisReq.DOCOEntry.FreeText.Contains("//R/") && (infant ? apisReq.DOCOEntry.FreeText.EndsWith("/I") : !apisReq.DOCOEntry.FreeText.EndsWith("/I")))
                {
                    travelDocuments.Add(new()
                    {
                        DocumentType = Constants.Application.RedressDocumentType,
                        Nationality = apisReq.DOCOEntry.FreeText.Split("//R/").Last().Split("///").Last(),
                        CountryOfResidence = apisReq.DOCOEntry.FreeText.Split("//R/").Last().Split("///").Last(),
                        DocumentNumber = apisReq.DOCOEntry.FreeText.Split("//R/").Last().Split("///").First(),
                    });

                }

                if (apisReq.DOCOEntry != null && apisReq.DOCOEntry.FreeText.Contains("//K/") && (infant ? apisReq.DOCOEntry.FreeText.EndsWith("/I") : !apisReq.DOCOEntry.FreeText.EndsWith("/I")))
                {
                    travelDocuments.Add(new()
                    {
                        DocumentNumber = apisReq.DOCOEntry.FreeText.Split("//K/").Last().Split("///").First(),
                        DocumentType = Constants.Application.DocumentType,
                        Nationality = apisReq.DOCOEntry.FreeText.Split("//K/").Last().Split("///").Last(),
                        CountryOfResidence = apisReq.DOCOEntry.FreeText.Split("//K/").Last().Split("///").Last()
                    });

                }

            }


            return travelDocuments.Any() ? travelDocuments : null;
        }

        private static void MapSegmentDetails(SegmentTypePNRBSegmentAir segment, ProductType product, string OdRefId, List<Segment> segmentList, PriceQuoteInfoGetResponse priceQuote, string connectionDuration, bool historyFlag)
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
                ScheduleChangeIndicator = historyFlag ? segment.ScheduleChangeIndicator: GetProductScheduleChangeIndicator(product)
            });
        }

        private static bool GetProductScheduleChangeIndicator(ProductType product) =>
            product?.ProductDetails?.Item is AirType2 productAir && productAir.ScheduleChangeIndicator;

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

        public static OrderDocument MapForOCN(OrderSearchRequest request, GetReservationRS response, Result<EAPI.Domain.Entities.Common.Response<EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType>> getDeltaSeatServiceResponse, Result<EAPI.Domain.Entities.Common.Response<List<RetrieveResponseType>>> getDeltaSeatTicketResponse)
        {
            var orderId = request.SearchFilterCriteria.OrderReferenceCriteria.OrderID;
            var reservation = (ReservationPNRB)response.Item;
            var priceQuote = response.PriceQuote?.Item != null ? (PriceQuoteInfoGetResponse)response.PriceQuote.Item : null;
            var history = reservation.History.Where(h => h.Items.Any(i => i.GetType().Name == "ReservationPNRBHistoryTransactionItineraryHistory")).OrderBy(o => o.Signature.SequenceNbr).Select(x => new { x.Signature, ItineraryHistory = x.Items.Where(i => i.GetType().Name == "ReservationPNRBHistoryTransactionItineraryHistory") });
            var recentHistory = history.Count() > 1 ? history.ElementAt(history.Count() - 2) : history?.FirstOrDefault();
            
            var dataList = new DataList()
            {
                CustomerDetails = [],
                OriginDestinationAssociations = [],
                OrderInstructions = [],
                SegmentDetails = [],
                PriceClass = [],
                Journey = []
            };
            if (reservation?.PassengerReservation?.Segments?.Segment != null)
            {
                MapOriginDestinationAssociations(
                    reservation.PassengerReservation.Segments.Segment,
                    dataList,
                    priceQuote,
                    historyFlag: request.SearchFilterCriteria.HistoryFlag);
            }
                dataList.PriceClass = MapPriceClass(dataList.SegmentDetails);
                dataList.Journey = MapJourney(dataList.OriginDestinationAssociations, dataList.SegmentDetails);            

            dataList.CustomerDetails = MapCustomerDetails(reservation, request.SearchFilterCriteria.CustomerDetails);

            UpdateDataList(dataList);

            return new OrderDocument()
            {
                BookingReference = new()
                {
                    ExternalSystem = Constants.Application.ExternalSystem,
                    ExternalRecordLocator = ((SegmentTypePNRBSegmentAir)reservation.PassengerReservation.Segments?.Segment?.FirstOrDefault()?.Item)?.AirlineRefId?.Split("*").Last(),
                    RLOC = request.SearchFilterCriteria.BookingReferenceCriteria.BookingReference.RLOC,
                    RLOCCreateDate = reservation.BookingDetails.CreationTimestamp.ToString(Constants.Application.DateFormat),
                    RLOCCreateDateTime = reservation.BookingDetails.CreationTimestamp.ToString(Constants.Application.TimeOnlyFormat)
                },
                CreationDateTime = new()
                {
                    LocalDateTime = reservation.BookingDetails.CreationTimestamp.ToString()
                },
                OrderHistory =
                [
                    new()
                    {
                        DataList = new()
                        {
                            SegmentDetails = MapSegments(recentHistory?.ItineraryHistory?.Select(x => x as ReservationPNRBHistoryTransactionItineraryHistory), reservation?.PassengerReservation?.Segments?.Segment, true).ToArray()
                        },
                        ChangeType = MapChangeType(history?.Last().ItineraryHistory.OfType<ReservationPNRBHistoryTransactionItineraryHistory>(),  history.SkipLast(1).SelectMany(x => x.ItineraryHistory.OfType<ReservationPNRBHistoryTransactionItineraryHistory>()), reservation),
                        ChangeDateTime =  recentHistory.Signature.HistoryTimestamp.ToString(Constants.Application.ZuluDateTimeFormat)
                    },
                ],
                DocType = Constants.Application.OrderDocType,
                Id = orderId,
                DataList = dataList,
                PartitionKey = orderId,
                Parties = request.SearchFilterCriteria.Parties,
                TimeLimits = reservation.PassengerReservation.TicketingInfo.FutureTicketing?.Select(x => new TimeLimit { Limit = GetTTL(x) }),
                OrderNumber = new()
                {
                    OrderId = orderId,
                    OwnerAirline = Constants.Application.OwnerAirline
                },
                OrderRemarks = MapOrderRemarks(reservation.Remarks),
                OrderItems = MapOrderItems(orderId, reservation, priceQuote, dataList, getDeltaSeatServiceResponse, getDeltaSeatTicketResponse, true),
                OrderStatus = reservation.PassengerReservation.TicketingInfo?.TicketDetails != null ? Constants.Application.OrderStatusConfirmed: string.Empty
            };
        }

        private static bool IsTicketExchanged(ReservationPNRB reservation)
        {
            if (reservation.History == null || reservation.History.Length < 2)
                return false;

            var orderedHistory = reservation.History
                .OrderByDescending(x => x?.Signature?.SequenceNbr)
                .ToList();

            int index = 0;
            while (index < orderedHistory.Count && IsTicketFact(orderedHistory[index], "A3S"))
                index++;

            return index > 0 && index < orderedHistory.Count && IsTicketFact(orderedHistory[index], "X3S*");
        }

        private static bool IsTicketFact(ReservationPNRBHistoryTransaction history, string action)
        {
            return history?.Items?.OfType<FactHistoryType>()
                .Any(x => string.Equals(x?.HistoryAction, action, StringComparison.Ordinal) && string.Equals(x?.Code, "TKNE", StringComparison.Ordinal)) == true;
        }

        private static bool IsHoldBookingIssuanceTrigger(ReservationPNRBHistoryTransaction latestHistoryTransaction, ReservationPNRBHistoryTransaction previousHistoryTransaction)
        {
            var hasTawWithDate = previousHistoryTransaction?.Items?.OfType<TicketingHistoryType>()?.Any(x => !string.IsNullOrWhiteSpace(x.OriginalTicketDetails) && x.OriginalTicketDetails.StartsWith("TAW", StringComparison.Ordinal) && x.OriginalTicketDetails.Length > 4) == true;

            var hasTkneA3S = latestHistoryTransaction?.Items?.OfType<FactHistoryType>()?.Any(x => x?.Code == "TKNE" && x?.HistoryAction == "A3S") == true;

            return hasTawWithDate && hasTkneA3S;
        }

        public static OrderDocument MapForOrderHistory(OrderSearchRequest request, GetReservationRS response, Result<EAPI.Domain.Entities.Common.Response<EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType>> getDeltaSeatServiceResponse, Result<EAPI.Domain.Entities.Common.Response<List<RetrieveResponseType>>> getDeltaSeatTicketResponse)
        {
            var orderId = request.SearchFilterCriteria.OrderReferenceCriteria.OrderID;
            var reservation = (ReservationPNRB)response.Item;
            var priceQuote = response.PriceQuote?.Item != null ? (PriceQuoteInfoGetResponse)response.PriceQuote.Item : null;

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

            if (reservation.PassengerReservation.Segments.Segment != null)
            {
                MapOriginDestinationAssociations(
                    reservation.PassengerReservation.Segments.Segment,
                    dataList,
                    priceQuote,
                    historyFlag: request.SearchFilterCriteria.HistoryFlag);
                dataList.PriceClass = MapPriceClass(dataList.SegmentDetails);
                dataList.Journey = MapJourney(dataList.OriginDestinationAssociations, dataList.SegmentDetails);
            }
            dataList.CustomerDetails = MapCustomerDetails(reservation, request.SearchFilterCriteria.CustomerDetails);

            UpdateDataList(dataList);

            return new OrderDocument()
            {
                BookingReference = new()
                {
                    ExternalSystem = Constants.Application.ExternalSystem,
                    ExternalRecordLocator = ((SegmentTypePNRBSegmentAir)reservation.PassengerReservation.Segments?.Segment?.FirstOrDefault()?.Item)?.AirlineRefId?.Split("*").Last(),
                    RLOC = request.SearchFilterCriteria.BookingReferenceCriteria.BookingReference.RLOC,
                    RLOCCreateDate = reservation.BookingDetails.CreationTimestamp.ToString(Constants.Application.DateFormat),
                    RLOCCreateDateTime = reservation.BookingDetails.CreationTimestamp.ToString(Constants.Application.TimeOnlyFormat)
                },
                CreationDateTime = new()
                {
                    LocalDateTime = reservation.BookingDetails.CreationTimestamp.ToString()
                },
                OrderHistory = MapOrderHistory(reservation),
                DocType = Constants.Application.OrderDocType,
                Id = orderId,
                DataList = dataList,
                PartitionKey = orderId,
                Parties = request.SearchFilterCriteria.Parties,
                TimeLimits = reservation.PassengerReservation.TicketingInfo.FutureTicketing?.Select(x => new TimeLimit { Limit = GetTTL(x) }),
                OrderNumber = new()
                {
                    OrderId = orderId,
                    OwnerAirline = Constants.Application.OwnerAirline
                },
                OrderItems = MapOrderItems(orderId, reservation, null, dataList, getDeltaSeatServiceResponse, getDeltaSeatTicketResponse, false, warnings, true),
                OrderStatus = reservation.PassengerReservation.TicketingInfo.TicketDetails != null ? Constants.Application.OrderStatusConfirmed : string.Empty
            };
        }

        private static IEnumerable<Domain.OrderHistory> MapOrderHistory(ReservationPNRB reservation)
        {
            var orderHistories = new List<Domain.OrderHistory>();
            var index = 1;
            int sequenceCount = 0;
            int isHoldBookingTicketingSequenceCount = 0;

            foreach (var histories in reservation.History.OrderBy(x => x.Signature.SequenceNbr))
            {
                var initialTicketingHistoryItem = reservation.History.FirstOrDefault(x => x.Signature.SequenceNbr == 1)?.Items.FirstOrDefault(h => h.GetType().Name == "TicketingHistoryType") as TicketingHistoryType;
                var paxCount = reservation.History.First(x => x.Signature.SequenceNbr == 1).Items.Count(h => h is PassengerNameHistoryType);
                var itineraryHistory = histories.Items.OfType<ReservationPNRBHistoryTransactionItineraryHistory>();
                var ticketingHistory = histories.Items.OfType<TicketingHistoryType>();
                var nameHistories = histories.Items.Where(h => h.GetType().Name == "PassengerNameHistoryType").Select(x => x as PassengerNameHistoryType);
                var factHistory = histories.Items.OfType<FactHistoryType>();
                var seatHistory = histories.Items.OfType<SeatsHistoryType>();
                var remarkHistory = histories.Items.Where(h => h.GetType().Name == "RemarkHistoryType").Select(x => x as RemarkHistoryType);

                // fallback: last previous transaction with itinerary                
                var previousItineraryHistory = reservation.History.Where(ht => ht.Signature.SequenceNbr <= histories.Signature.SequenceNbr && ht.Items.Any(i => i.GetType().Name == "ReservationPNRBHistoryTransactionItineraryHistory")).OrderByDescending(ht => ht.Signature.SequenceNbr).First().Items.Where(x => x.GetType().Name == "ReservationPNRBHistoryTransactionItineraryHistory").Select(x => x as ReservationPNRBHistoryTransactionItineraryHistory);

                if (histories.Signature.HistoryFrom != "API" && itineraryHistory.Any() && itineraryHistory.SelectMany(x => x.Items).Select(x => x as AirSegmentHistoryType).Select(x => x.HistoryAction).Contains("SC"))
                {
                    //OCN
                    var changeType = MapChangeType(itineraryHistory, previousItineraryHistory);

                    orderHistories.Add(new Domain.OrderHistory()
                    {
                        DataList = new()
                        {
                            SegmentDetails = MapSegments(itineraryHistory, reservation.PassengerReservation.Segments.Segment, true),
                        },
                        ChangeType = changeType,
                        Version = index.ToString(),
                        ChangeDateTime = histories.Signature.HistoryTimestamp.ToString(Constants.Application.ZuluDateTimeFormat),
                        ReasonCode = changeType == ChangeTypeEnum.FlightCancellation ? "116" : "100"
                    });
                    index++;
                }
                else
                {
                    if (initialTicketingHistoryItem?.OriginalTicketDetails == "TAW/")
                    {
                        if (histories.Signature.SequenceNbr > 1 && !itineraryHistory.Any() && ticketingHistory.Any())
                        {
                            continue;
                        }
                        else if (histories.Signature.SequenceNbr > 1 && !itineraryHistory.Any() && !ticketingHistory.Any() && !nameHistories.Any())
                        {

                            if ((factHistory != null && factHistory.Any(x => x.Type == "SSR") && !factHistory.Any(x => x.Code == "DOCS") && !factHistory.Any(x => x.Code == "DOCO") && ((factHistory.Any(x => x.ActionCode != "KK") && (!factHistory.Any(x => x.ActionCode == "NO"))) || seatHistory.Any())))
                            {
                                continue;
                            }
                            else if (factHistory != null && factHistory.Any(x => x.Type == "SSR" && (x.Code == "DOCS" || x.Code == "DOCO")) && factHistory.Any(x => x.HistoryAction == "X3S"))
                            {
                                continue;
                            }
                            else if ((factHistory.Any(x => x.Type == "SSR" && x.HistoryAction == "A3S" && x.FullText.Contains("/DB/")) || (factHistory.Any(x => x.Type == "OSI"))) && !itineraryHistory.Any()) continue;

                            else if (histories.Signature.SequenceNbr > 1 && !ticketingHistory.Any() && remarkHistory.Any() && !remarkHistory.Any(x => x.RemarkText.Contains("SPLIT TO"))) { continue; }

                        }
                        else if (factHistory.Any(x => x.Type == "SSR" && x.HistoryAction == "A3S" && x.Code == "TKNE" && x.FullText.Contains("TKNE")) && !itineraryHistory.Any() && ticketingHistory.Any()) { continue; }
                    }

                    else
                    {
                        var latestOrderHistory = orderHistories.OrderByDescending(x => x.Version)?.FirstOrDefault();
                        sequenceCount = latestOrderHistory?.ChangeType == ChangeTypeEnum.PaymentFailure ? (sequenceCount == 0 ? histories.Signature.SequenceNbr : sequenceCount) : 0;
                        isHoldBookingTicketingSequenceCount = latestOrderHistory?.ChangeType == ChangeTypeEnum.OrderItemAdded && ticketingHistory.Any() ? (isHoldBookingTicketingSequenceCount == 0 ? histories.Signature.SequenceNbr : isHoldBookingTicketingSequenceCount) : 0;
                        var distinctPassengerPaxCount = reservation.PassengerReservation.Passengers.Passenger.Select(x => x.passengerType).Distinct().Count();

                        if (histories.Signature.SequenceNbr > 1 && (histories.Signature.SequenceNbr < (isHoldBookingTicketingSequenceCount + distinctPassengerPaxCount) - 1) && ticketingHistory.Any())
                        {
                            continue;
                        }
                        else if (factHistory.Any(x => x.Type == "SSR" && x.HistoryAction == "A3S" && x.FullText.Contains("/DB/") && factHistory.Any(x => x.Type == "OSI")) && !itineraryHistory.Any()) continue;
                        else if (factHistory.Any(x => x.Type == "SSR" && x.HistoryAction == "A4S" && x.Code == "INFT" && !itineraryHistory.Any() && !ticketingHistory.Any())) continue;
                        else if (latestOrderHistory != null && latestOrderHistory.ChangeType == ChangeTypeEnum.PaymentFailure)
                        {
                            if (ticketingHistory.Any(x => x.HistoryAction == "X7") && ticketingHistory.Any(x => x.HistoryAction == "A7"))
                            {
                                if (histories.Signature.SequenceNbr < (sequenceCount + distinctPassengerPaxCount) - 1)
                                {
                                    continue;
                                }
                            }
                        }

                        else if (factHistory.Any(x => x.Type == "SSR" && x.HistoryAction == "A3S" && x.Code == "TKNE" && (x.FullText.Contains("TKNE")) || x.FullText.Contains("/DB")) && !ticketingHistory.Any() && !nameHistories.Any()) { continue; }
                    }
                }

                var segDetails = itineraryHistory.Any() ? MapSegments(itineraryHistory, reservation.PassengerReservation.Segments.Segment) : MapSegments(previousItineraryHistory, reservation.PassengerReservation.Segments.Segment);

                orderHistories.Add(new Domain.OrderHistory()
                {
                    DataList = new()
                    {
                        SegmentDetails = segDetails,
                        CustomerDetails = MapCustomerDetails(reservation, histories.Items.OfType<FactHistoryType>().ToArray(), orderHistories, nameHistories),
                        OrderInstructions = MapOrderInstructions(factHistory.ToArray())
                    },
                    OrderItems = seatHistory.Any() ? MapSeatOrderItemsForHistory(seatHistory, segDetails) : null,
                    BookingReference = remarkHistory != null ? MapBookingReference(reservation, histories.Items.OfType<RemarkHistoryType>()) : null,
                    ChangeType = MapOrderHistoryChangeType(histories),
                    Version = index.ToString(),
                    ChangeDateTime = MapUtcDateTime(histories.Signature.HistoryTimestamp).ToString(Constants.Application.ZuluDateTimeFormat)
                });
                index++;
            }

            return orderHistories;
        }

        private static IEnumerable<OrderInstructions> MapOrderInstructions(FactHistoryType[] factHistoryTypes)
        {
            int instructionCounter = 1;
            var excludedCodes = new[] { "DOCS", "DOCO", "CHLD", "INF" };

            if (factHistoryTypes != null)
            {
                var ssrHistories = factHistoryTypes.Where(f => f.Type == "SSR" && f.HistoryAction == "A4S" && f.ActionCode == "KK" && !excludedCodes.Contains(f.Code)).Select(f => new OrderInstructions
                {
                    ItemText = f.Type,
                    ItemCode = f.Code,
                    InstructionId = $"OI{instructionCounter++}"
                });

                var failedSSRHistories = factHistoryTypes.Where(f => f.Type == "SSR" && f.HistoryAction == "A4S" && f.ActionCode == "NO" && !excludedCodes.Contains(f.Code)).Select(f => new OrderInstructions
                {
                    ItemText = f.Type,
                    ItemCode = f.Code,
                    InstructionId = $"OI{instructionCounter++}"
                });

                return ssrHistories.Concat(failedSSRHistories);
            }

            return Enumerable.Empty<OrderInstructions>();
        }

        private static IEnumerable<OrderItems> MapSeatOrderItemsForHistory(IEnumerable<SeatsHistoryType> seatsHistories, IEnumerable<Segment> segments)
        {
            return seatsHistories.Select(s =>
            {
                return new OrderItems()
                {
                    Services = [new() {
                        Product = new() { ProductType = Product.ProductTypes.SeatEnum },
                        CurrentSeatAssignment = s.SeatNumber,
                        SegmentRefIds = [segments.First(x => x.OriginAirportCode.Is(s.BoardCity) && x.DestinationAirportCode.Is(s.OffCity) && Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).Date == Convert.ToDateTime(s.FlightDate).Date && x.MarketingFlightNumber == s.FlightNumber).SegmentId]
                    }]
                };
            });
        }

        private static BookingReference MapBookingReference(ReservationPNRB reservation, IEnumerable<RemarkHistoryType> remarkHistories)
        {
            string rLoc = reservation.BookingDetails.RecordLocator;

            var remark = remarkHistories.FirstOrDefault();
            if (remark == null || string.IsNullOrWhiteSpace(remark.RemarkText)) return null;

            var bookingReference = new BookingReference
            {
                RLOC = rLoc,
                ExternalSystem = "1S"
            };

            // SPLIT TO → child RLOC is shared, main PNR is current rLoc
            if (remark.RemarkText.Contains("SPLIT TO", StringComparison.OrdinalIgnoreCase))
            {
                var parts = remark.RemarkText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var childRloc = parts.FirstOrDefault(p => p.Length == 6 && p.All(char.IsLetterOrDigit));

                bookingReference.SplitRLOC = new SplitRLOC
                {
                    ParentRLOC = rLoc,
                    ParentRLOCLink = true,
                    ChildRLOCDetails = new List<ChildRLOCDetails>
                    {
                        new ChildRLOCDetails
                        {
                            ChildRLOC = childRloc,
                            ChildRLOCLink = true,
                           // SplitDateTime = remark.HistoryTimestamp?.ToString("yyyy-MM-ddTHH:mm:ss")
                        }
                    }
                };
            }

            else if (remark.RemarkText.Contains("SPLIT FR", StringComparison.OrdinalIgnoreCase))
            {
                var parts = remark.RemarkText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var parentRloc = parts.FirstOrDefault(p => p.Length == 6 && p.All(char.IsLetterOrDigit));

                bookingReference.SplitRLOC = new SplitRLOC
                {
                    ParentRLOC = parentRloc,
                    ParentRLOCLink = true,
                    ChildRLOCDetails = new List<ChildRLOCDetails>
                    {
                        new ChildRLOCDetails
                        {
                            ChildRLOC = rLoc,
                            ChildRLOCLink = true,
                            //SplitDateTime = remark.HistoryTimestamp?.ToString("yyyy-MM-ddTHH:mm:ss")
                        }
                    }
                };
            }

            return bookingReference;
        }

        private static IEnumerable<Customer> MapCustomerDetails(ReservationPNRB reservation, FactHistoryType[] factHistoryTypes, IEnumerable<Domain.OrderHistory> orderHistories, IEnumerable<PassengerNameHistoryType> passengerNameHistories)
        {
            var passengers = reservation.PassengerReservation.Passengers.Passenger;

            var passengerList = new List<Customer>();
            Customer customer = null;

            foreach (var pax in passengers)
            {
                var nameHistories = reservation?.History.SelectMany(x => x.Items.OfType<PassengerNameHistoryType>()).FirstOrDefault(x => x.HistoryAction == "XN" && x.NameSeqID.ToString() == pax.nameAssocId);

                var paxNames = new Dictionary<string, string>();
                paxNames.Add($"{pax.LastName} {pax.FirstName}", $"{nameHistories?.LastName} {nameHistories?.FirstName}");
                FactHistoryType matchingFactHistory = null;
                var nameNumber = pax.nameId.Split('.');
                var lastNameNumber = nameNumber[0];
                var firstNameNumber = nameNumber[1];

                var factHistories = factHistoryTypes.Where(x => x.Type == "SSR" && x.Code == "DOCS" && x.HistoryAction == "A3S" && x.FullText.Contains("/DB/"));

                // Picking DOB and Gender from FactHistories
                if (factHistories.Any())
                {
                    if (nameHistories != null) matchingFactHistory = factHistories.FirstOrDefault(f => paxNames.Values.Any(v => v.Contains($"{f.FullText.Split('/')[^2]} {f.FullText.Split('/')[^1]}")));
                    else matchingFactHistory = factHistories.FirstOrDefault(f => paxNames.Keys.Contains($"{f.FullText.Split('/')[^2]} {f.FullText.Split('/')[^1]}"));

                    var custDetails = matchingFactHistory.FullText.Split('/');

                    customer = new Customer
                    {
                        FirstName = custDetails[5],
                        LastName = custDetails[4],
                        DateOfBirth = DateTime.ParseExact(custDetails[2], "ddMMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                        Gender = custDetails[3],
                        FirstNameNumber = firstNameNumber,
                        LastNameNumber = lastNameNumber,
                        Ptc = pax.passengerType
                    };
                }
                // Picking DOB and Gender from Previous Histories for Name Corrrection Scenarios
                else if (nameHistories != null && !factHistories.Any())
                {
                    var newNameHistories = passengerNameHistories.FirstOrDefault(x => x.HistoryAction == "AN" && x.NameSeqID.ToString() == pax.nameAssocId);

                    //picking from previous histories dob and gender
                    var latestOrderHistory = orderHistories.OrderByDescending(x => x.Version).FirstOrDefault();

                    if (newNameHistories != null)
                    {
                        var custDetails = latestOrderHistory.DataList.CustomerDetails.FirstOrDefault(x => x.FirstName == nameHistories.FirstName && x.LastName == nameHistories.LastName);

                        customer = new Customer
                        {
                            FirstName = newNameHistories.FirstName,
                            LastName = newNameHistories.LastName,
                            DateOfBirth = custDetails.DateOfBirth,
                            Gender = custDetails.Gender,
                            FirstNameNumber = firstNameNumber,
                            LastNameNumber = lastNameNumber,
                            Ptc = pax.passengerType
                        };
                    }
                    //Changes happended to pnr before and after performing Name Correction
                    else
                    {
                        Customer custDetails = null;

                        var latestOrderHistories = orderHistories.OrderByDescending(x => x.Version).FirstOrDefault();
                        custDetails = latestOrderHistories?.DataList.CustomerDetails.FirstOrDefault(x => $"{x.LastName} {x.FirstName}" == paxNames.Keys.First());

                        if (custDetails == null)
                        {
                            custDetails = latestOrderHistories?.DataList.CustomerDetails.FirstOrDefault(x => $"{x.LastName} {x.FirstName}" == paxNames.Values.First());
                        }

                        customer = new Customer
                        {
                            FirstName = custDetails?.FirstName,
                            LastName = custDetails?.LastName,
                            DateOfBirth = custDetails?.DateOfBirth,
                            Gender = custDetails?.Gender,
                            FirstNameNumber = firstNameNumber,
                            LastNameNumber = lastNameNumber,
                            Ptc = pax.passengerType
                        };
                    }
                }
                else
                {
                    //picking from previous histories dob and gender
                    var latestOrderHistory = orderHistories.OrderByDescending(x => x.Version).FirstOrDefault();
                    var custDetails = latestOrderHistory.DataList.CustomerDetails.FirstOrDefault(x => x.FirstName == pax.FirstName && x.LastName == pax.LastName);

                    customer = new Customer
                    {
                        FirstName = pax.FirstName,
                        LastName = pax.LastName,
                        DateOfBirth = custDetails.DateOfBirth,
                        Gender = custDetails.Gender,
                        FirstNameNumber = firstNameNumber,
                        LastNameNumber = lastNameNumber,
                        Ptc = pax.passengerType
                    };
                }

                customer.TravelDocuments = new List<TravelDocument>();

                if (factHistoryTypes != null)
                {
                    IEnumerable<FactHistoryType> matchingDocs = null;

                    if (pax.passengerType == "INF")
                    {
                        var docInfo = factHistoryTypes.Where(item => item.Type == "SSR" && item.HistoryAction == "A3S" && (item.Code == "DOCS" || item.Code == "DOCO"));
                        var infantDoc = docInfo.Where(x => x.FullText.EndsWith("I"));
                        var paxLinkedwithInfant = passengers.Where(x => x.withInfant == true);
                        var matchInfantDoc = paxLinkedwithInfant.Where(x => x.SpecialRequests.APISRequest.Any(req => req.DOCSEntry?.Forename == pax.FirstName && req.DOCSEntry?.Surname == pax.LastName));
                        matchingDocs = infantDoc.Where(x => x.NameAssociations.Contains(matchInfantDoc.First().nameAssocId));

                    }
                    else
                    {
                        matchingDocs = factHistoryTypes.Where(item => item.Type == "SSR" && item.HistoryAction == "A3S" && (item.Code == "DOCS" || item.Code == "DOCO")).Where(doc => doc.NameAssociations.Contains(pax.nameAssocId));
                    }

                    foreach (var matchingDoc in matchingDocs)
                    {
                        if (matchingDoc != null)
                        {
                            var fullText = matchingDoc.FullText;
                            var docParts = fullText.Split(new[] { "/P/", "/C/", "/R/", "/K/", "/V/" }, StringSplitOptions.None);

                            if (docParts.Length > 1)
                            {
                                var documentInfo = docParts[1];
                                var details = documentInfo.Split('/');

                                if (details.Last() == "I" && pax.passengerType != "INF")
                                {
                                    continue;
                                }

                                if (fullText.Contains("/P/"))
                                {
                                    var travelDoc = new TravelDocument
                                    {
                                        DocumentType = "PT",
                                        DocumentNumber = details[1],
                                        CountryOfIssue = details[0],
                                        CountryOfResidence = details[2],
                                        DateOfBirth = DateTime.ParseExact(details[3], "ddMMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                                        Gender = details[4],
                                        ExpirationDate = DateTime.ParseExact(details[5], "ddMMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                                        PersonName = new()
                                        {
                                            FirstName = details[7],
                                            LastName = details[6],
                                        },
                                        Nationality = details[2]
                                    };
                                    customer.TravelDocuments.Add(travelDoc);
                                }
                                else if (fullText.Contains("/C/"))
                                {
                                    var travelDoc = new TravelDocument
                                    {
                                        DocumentType = "GC",
                                        DocumentNumber = details[1],
                                        CountryOfIssue = details[2],
                                        CountryOfResidence = details[0],
                                        DateOfBirth = DateTime.ParseExact(details[3], "ddMMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                                        Gender = details[4],
                                        ExpirationDate = DateTime.ParseExact(details[5], "ddMMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                                        PersonName = new()
                                        {
                                            FirstName = details[7],
                                            LastName = details[6]
                                        },
                                        Nationality = details[0]
                                    };
                                    customer.TravelDocuments.Add(travelDoc);
                                }
                                else if (fullText.Contains("/R/"))
                                {
                                    var travelDoc = new TravelDocument
                                    {
                                        DocumentType = "AEA",
                                        DocumentNumber = details[0],
                                        CountryOfResidence = details[3],
                                        Nationality = details[3]
                                    };
                                    customer.TravelDocuments.Add(travelDoc);
                                }
                                else if (fullText.Contains("/K/"))
                                {
                                    var travelDoc = new TravelDocument
                                    {
                                        DocumentType = "CR",
                                        DocumentNumber = details[0],
                                        CountryOfResidence = details[3],
                                        Nationality = details[3]
                                    };
                                    customer.TravelDocuments.Add(travelDoc);
                                }
                                else if (fullText.Contains("/V/"))
                                {
                                    var travelDoc = new TravelDocument
                                    {
                                        DocumentType = "VI",
                                        DocumentNumber = details[0],
                                        CountryOfIssue = details[1],
                                        CountryOfResidence = details[1],
                                        ExpirationDate = DateTime.ParseExact(details[5], "ddMMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                                        RegionOfIssuance = details[1],
                                        Nationality = details[1]
                                    };
                                    customer.TravelDocuments.Add(travelDoc);
                                }
                            }
                        }
                    }
                }

                passengerList.Add(customer);
            }

            return passengerList.Where(customer => customer != null);
        }


        private static ChangeTypeEnum MapOrderHistoryChangeType(ReservationPNRBHistoryTransaction history)
        {
            var ticketingHistory = history.Items.Where(h => h.GetType().Name == "TicketingHistoryType").Select(x => x as TicketingHistoryType);
            var seatsHistory = history.Items.OfType<SeatsHistoryType>().FirstOrDefault();
            var nameHistories = history.Items.Where(h => h.GetType().Name == "PassengerNameHistoryType").Select(x => x as PassengerNameHistoryType).ToList();
            var itineraryHistory = history.Items.OfType<ReservationPNRBHistoryTransactionItineraryHistory>().FirstOrDefault();
            var factHistory = history.Items.OfType<FactHistoryType>();
            var remarkHistory = history.Items.Where(h => h.GetType().Name == "RemarkHistoryType").Select(x => x as RemarkHistoryType);
            var excludedCodes = new[] { "DOCS", "DOCO", "CHLD", "INF" };

            if (ticketingHistory.Any())
            {
                if (itineraryHistory != null && ticketingHistory.First().TransactionIndicator == "TV" && ticketingHistory.First().HistoryAction == "A7")
                {
                    return ChangeTypeEnum.DocumentVoided;
                }
                else if (itineraryHistory != null && ticketingHistory.First().TransactionIndicator == "TR" && ticketingHistory.First().HistoryAction == "A7")
                {
                    return ChangeTypeEnum.DocumentRefunded;
                }
                else if (ticketingHistory.First().OriginalTicketDetails.StartsWith("TAW") && ticketingHistory.First().HistoryAction == "A7" && history.Signature.SequenceNbr == 1)
                {
                    return ChangeTypeEnum.OrderItemAdded;
                }
                else if (ticketingHistory.Any(x => x.HistoryAction == "X7") && ticketingHistory.Any(x => x.HistoryAction == "A7"))
                {
                    return ChangeTypeEnum.DocumentIssuance;
                }
                else if (itineraryHistory == null && ticketingHistory.First().TransactionIndicator == "TV")
                {
                    return ChangeTypeEnum.PaymentFailure;
                }
                else
                {
                    return ChangeTypeEnum.Unknown;
                }
            }

            else if ((nameHistories != null) && nameHistories.Any(n => n.HistoryAction == "XN") && nameHistories.Any(n => n.HistoryAction == "AN"))
            {
                return ChangeTypeEnum.NameCorrection;
            }
            else if (factHistory != null && factHistory.Any(x => x.Type == "SSR") && factHistory.Any(x => x.HistoryAction == "A3S") && factHistory.Any(x => x.Code == "DOCS" || x.Code == "DOCO") && factHistory.Any(x => new[] { "/P/", "/C/", "/R/", "/K/", "/V/" }.Any(substr => x.FullText.Contains(substr))))
            {
                return ChangeTypeEnum.IdentityDocumentDetailsUpdated;
            }
            else if (factHistory != null && (factHistory.Any(x => x.Type == "SSR" && x.HistoryAction == "A4S" && x.ActionCode == "KK" && !excludedCodes.Contains(x.Code)) || factHistory.Any(f => f.Type == "SSR" && f.HistoryAction == "A4S" && f.ActionCode == "NO" && !excludedCodes.Contains(f.Code))))
            {
                return ChangeTypeEnum.SpecialServiceRequestAdded;
            }

            else if (remarkHistory.Any()) return ChangeTypeEnum.SplitPNR;
            else if (seatsHistory != null) return ChangeTypeEnum.OrderItemAdded;
            else if (itineraryHistory != null && !ticketingHistory.Any()) return ChangeTypeEnum.DocumentExchanged;
            else return ChangeTypeEnum.Unknown;
        }

        private static ChangeTypeEnum MapChangeType(IEnumerable<ReservationPNRBHistoryTransactionItineraryHistory> latestHistory, IEnumerable<ReservationPNRBHistoryTransactionItineraryHistory> previousHistory, [Optional] ReservationPNRB reservation)
        {
            if(reservation is not null)
            {
                var latestHistoryTransaction = reservation.History.OrderByDescending(x => x?.Signature.SequenceNbr)?.FirstOrDefault();
                var previousHistoryTransaction = reservation.History.OrderByDescending(x => x?.Signature.SequenceNbr)?.Skip(1).FirstOrDefault();

                if (reservation.History.Length == 2 && latestHistoryTransaction?.Items?.OfType<TicketingHistoryType>()?.Any(x => string.Equals(x?.OriginalTicketDetails, "TAW/", StringComparison.Ordinal)) == true)
                {
                    throw new InvalidOperationException("No history found for the requested PNR.");
                }

                if (latestHistoryTransaction?.Items?.OfType<TicketingHistoryType>()?.Any(x => x.TransactionIndicator == "TV" && x.HistoryAction != "SC") == true)
                    return ChangeTypeEnum.DocumentVoided;

                else if (latestHistoryTransaction?.Items?.OfType<TicketingHistoryType>()?.Any(x => x.TransactionIndicator == "TR" && x.HistoryAction != "SC") == true)
                    return ChangeTypeEnum.DocumentRefunded;

                else if (IsHoldBookingIssuanceTrigger(latestHistoryTransaction, previousHistoryTransaction))
                    return ChangeTypeEnum.DocumentIssuance;

                else if (IsTicketExchanged(reservation))
                    return ChangeTypeEnum.DocumentExchanged;

            }

            if (latestHistory.Count() == 1)
            {
                var seg = latestHistory.First().Items.OfType<AirSegmentHistoryType>().First();

                if (seg.ActionCode == "UN")
                    return ChangeTypeEnum.FlightCancellation;

                else
                {
                    var matchedSeg = previousHistory.SelectMany(h => h.Items.OfType<AirSegmentHistoryType>()).Last(x => x.MarketingFlightNumber.Is(seg.MarketingFlightNumber));
                    if (matchedSeg.ArrivalAirport.Is(seg.ArrivalAirport) && matchedSeg.DepartureAirport.Is(seg.DepartureAirport) && (Convert.ToDateTime(matchedSeg.DepartureDateTime) != Convert.ToDateTime(seg.ScheduledDepartureDateTime) || Convert.ToDateTime(matchedSeg.ArrivalDateTime) != Convert.ToDateTime(seg.ScheduledArrivalDateTime)))
                        return ChangeTypeEnum.FlightDateTimeChange;
                    else
                        return ChangeTypeEnum.FlightRouteChange;
                }
            }
            else if (latestHistory.Count() == 2)
            {
                var segs = latestHistory.SelectMany(x => x.Items.OfType<AirSegmentHistoryType>());

                if (segs.Any(x => x.ActionCode == "UN") && segs.Any(x => x.ActionCode == "TK"))
                    return ChangeTypeEnum.FlightCancellation;

                if (segs.All(x => x.ActionCode == "UN"))
                    return ChangeTypeEnum.FlightCancellation;

                else
                    return ChangeTypeEnum.FlightDateTimeChange;
            }
            else
            {
                var segs = latestHistory.SelectMany(x => x.Items.OfType<AirSegmentHistoryType>());

                if (segs.All(x => x.ActionCode == "UN"))
                    return ChangeTypeEnum.FlightCancellation;

                else
                    return ChangeTypeEnum.FlightDateTimeChange;
            }
        }

        public static IEnumerable<Segment> MapSegments(IEnumerable<ReservationPNRBHistoryTransactionItineraryHistory> itineraryHistory, SegmentTypePNRBSegment[] getReservationSegments, [Optional] bool isOCN)
        {
            var segmentDetails = new List<Segment>();

            foreach (var segment in itineraryHistory.SelectMany(x => x.Items.OfType<AirSegmentHistoryType>()))
            {
                bool isSegmentHistoryAction = itineraryHistory.SelectMany(x => x.Items.OfType<AirSegmentHistoryType>()).Any(x => x.HistoryAction == "XS") && itineraryHistory.SelectMany(x => x.Items.OfType<AirSegmentHistoryType>()).Any(x => x.HistoryAction != "XS") || isOCN;

                if (isSegmentHistoryAction)
                {
                    if (segment.HistoryAction == "XS") continue;
                    var itineraryHistorysegmentsCount = itineraryHistory.SelectMany(x => x.Items.OfType<AirSegmentHistoryType>()).Where(x => x.HistoryAction != "XS");
                    if ((itineraryHistorysegmentsCount?.Count() ?? 0) != (getReservationSegments?.Count() ?? 0))
                    {
                        var airSegments = getReservationSegments?.Select(x => x.Item)?.OfType<SegmentTypePNRBSegmentAir>();
                        var matchingAirSegments = airSegments?.FirstOrDefault(x => !(x.ClassOfService == segment.ClassOfService && x.MarketingFlightNumber == segment.MarketingFlightNumber && x.DepartureAirport == segment.DepartureAirport && x.ArrivalAirport == segment.ArrivalAirport && Convert.ToDateTime(x.DepartureDateTime) == Convert.ToDateTime(segment.DepartureDateTime) && Convert.ToDateTime(x.ArrivalDateTime) == Convert.ToDateTime(segment.ArrivalDateTime)));

                        if (matchingAirSegments != null)
                        {
                            segmentDetails.Add(new Segment()
                            {
                                OriginAirportCode = matchingAirSegments.DepartureAirport,
                                DestinationAirportCode = matchingAirSegments.ArrivalAirport,
                                ScheduledDepartureTime = new()
                                {
                                    LocalDateTime = matchingAirSegments.DepartureDateTime
                                },
                                ScheduledArrivalTime = new()
                                {
                                    LocalDateTime = matchingAirSegments.ArrivalDateTime
                                },
                                MarketingCarrier = matchingAirSegments.MarketingAirlineCode,
                                MarketingFlightNumber = matchingAirSegments.MarketingFlightNumber?.TrimStart('0'),
                                OperatingCarrier = matchingAirSegments.OperatingAirlineCode,
                                OperatingFlightNumber = matchingAirSegments.OperatingFlightNumber?.TrimStart('0'),
                                ClassOfService = matchingAirSegments.ClassOfService,
                                Status = new() { Code = matchingAirSegments.ActionCode },
                            });
                        }
                    }
                }

                if (segment.ActionCode == "SS" && segment.HistoryAction == "AS") segment.ActionCode = "HK";
                if (segment.ActionCode == "HK" && segment.HistoryAction == "XS") segment.ActionCode = "HX";

                segmentDetails.Add(new Segment()
                {
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
                    ScheduleChangeIndicator = segment.ScheduleChangeIndicator
                });
            }

            return segmentDetails;
        }

        private static DateTime MapUtcDateTime(DateTime creationDateTimeStamp)
        {
            var cstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(creationDateTimeStamp, cstTimeZone);
        }
    }
}
