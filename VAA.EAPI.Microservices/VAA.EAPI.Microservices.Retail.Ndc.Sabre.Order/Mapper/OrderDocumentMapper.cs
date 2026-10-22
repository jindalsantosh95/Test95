using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;
using OrderFareComponent = VAA.EAPI.Domain.Virgin.Models.Order.FareComponent;


namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mapper
{
    public class OrderDocumentMapper
    {
        public static OrderDocument Mapper(OrderCreateRequest orderCreateRequest, GetReservationRS orderRs)
        {
            var warninglist = new List<Warning>();
            string pnr = (orderRs.Item as ReservationPNRB).BookingDetails.RecordLocator;
            var deltaPNR = ((orderRs.Item as ReservationPNRB).PassengerReservation.Segments.Segment.First().Item as SegmentTypePNRBSegmentAir).AirlineRefId[^6..];
            string orderId = $"VS932{pnr}{DateTime.UtcNow:yy}";

            return new OrderDocument()
            {
                BookingReference = new()
                {
                    ExternalSystem = "1S",
                    ExternalRecordLocator = deltaPNR,
                    RLOC = pnr,
                    RLOCCreateDate = DateTime.UtcNow.ToString(Constants.Application.DateFormat),
                    RLOCCreateDateTime = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
                },
                CreationDateTime = new()
                {
                    UtcDateTime = MapUtcDateTime((orderRs.Item as ReservationPNRB).BookingDetails.CreationTimestamp).ToString(Constants.Application.ZuluDateTimeFormat),
                },
                DocType = Constants.Application.OrderDocType,
                Id = orderId,
                PartitionKey = orderId,
                Parties = orderCreateRequest.Parties,
                PointOfSale = orderCreateRequest.Pos,
                TimeLimits = orderCreateRequest.TimeLimits,
                OrderNumber = new()
                {
                    OrderId = orderId,
                    OwnerAirline = Constants.Application.OwnerAirline
                },
                OrderRemarks = orderCreateRequest.OrderRemarks,
                OrderItems = MapOrderItems(orderCreateRequest, orderId, orderRs, warninglist),
                Warnings = warninglist.Any() ? warninglist : null,
                DataList = GetDataList(orderCreateRequest.DataList, warninglist, orderRs),
                FareTypeRequest = (VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain.FareTypeRequest)orderCreateRequest, // Using the explicit operator here,
                Version = 1,
                ChangeDateTime = new()
                {
                    UtcDateTime = MapUtcDateTime((orderRs.Item as ReservationPNRB).BookingDetails.CreationTimestamp).ToString(Constants.Application.ZuluDateTimeFormat)
                },
                ChangeType = orderCreateRequest.ChangeType
            };
        }

        private static DataList GetDataList(DataList dataList, List<Warning> warning, GetReservationRS response)
        {
            var reservation = (ReservationPNRB)response.Item;
            var passengersWithInfants = reservation.PassengerReservation.Passengers.Passenger.Where(x => x.withInfant);
            var infantDetails = dataList.CustomerDetails.Where(x => x.Ptc == Constants.Application.Infant);

            MapOriginDestinationAssociations(reservation.PassengerReservation.Segments.Segment, dataList);
            dataList.Journey = MapJourney(dataList.OriginDestinationAssociations, dataList.SegmentDetails);

            UpdateDataList(dataList);

            foreach (var passengerWithInfant in passengersWithInfants)
            {
                var infantFreeText = passengerWithInfant.SpecialRequests.GenericSpecialRequest.FirstOrDefault(req => req.Code == "INFT")?.FreeText;

                if (infantFreeText != null)
                {
                    var freeTextParts = infantFreeText.Split('/');
                    string infantLastName = freeTextParts[1];
                    string infantFirstName = freeTextParts[2];

                    var matchingInfantDetails = infantDetails.FirstOrDefault(x => x.LastName.Is(infantLastName) && x.FirstName.Is(infantFirstName));

                    if (matchingInfantDetails != null)
                    {
                        var adultPassenger = dataList.CustomerDetails.FirstOrDefault(x => x.FirstName.Is(passengerWithInfant.FirstName) && x.LastName.Is(passengerWithInfant.LastName));

                        if (adultPassenger != null)
                        {
                            adultPassenger.LinkedInfantId = matchingInfantDetails.CustId;
                            matchingInfantDetails.AssociatedWithAdult = adultPassenger.CustId;

                        }
                    }
                }
            }

            if (warning.Any())
            {
                var orderInstructions = new List<OrderInstructions>();

                if (dataList.OrderInstructions.Any())
                {
                    foreach (var oi in dataList.OrderInstructions)
                    {
                        var itemText = oi.ItemText.Split("/");
                        
                        if (!warning.Any(w => w.Details == oi.ItemCode && w.Message.Contains(itemText[1]) && w.Message.Contains(itemText[2])))
                        {
                            orderInstructions.Add(oi);
                        }
                    }
                }

                dataList.OrderInstructions = orderInstructions;
            }

            return dataList;
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

        private static void MapOriginDestinationAssociations(SegmentTypePNRBSegment[] segments, DataList dataList)
        {
            var originDestinationAssociationsList = new List<OriginDestinationAssociation>();
            string departureAirport = "";
            string arrivalAirport = "";
            string departureDateTime = "";
            var segmentDetailsList = new List<Segment>();

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Item.GetType().Name == "Arunk")
                    continue;

                var segment = (SegmentTypePNRBSegmentAir)segments[i].Item;
                var index = originDestinationAssociationsList.Count;
                var odRefId = $"{Constants.Application.OriginDestinationIdPrefix}{index + 1}";
                var ond = new OriginDestinationAssociation();
                var connectionDuration = ((!segment.outboundConnection && !segment.inboundConnection) || (segment.outboundConnection && !segment.inboundConnection)) ? null : DateTime.Parse(segment.DepartureDateTime).Subtract(DateTime.Parse(((SegmentTypePNRBSegmentAir)segments[i - 1].Item).ArrivalDateTime)).TotalMinutes.ToString();

                var orderDocSegment = dataList.SegmentDetails.First(x => x.OriginAirportCode == segment.DepartureAirport && x.DestinationAirportCode == segment.ArrivalAirport);

                MapSegmentDetails(segment, odRefId, segmentDetailsList, connectionDuration, orderDocSegment);

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

        private static void MapSegmentDetails(SegmentTypePNRBSegmentAir segment, string OdRefId, List<Segment> segmentList, string connectionDuration, Segment orderDocSegment)
        {
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
                FlightDuration = GetDuration(segment.HiddenStop, segment.ElapsedTime),
                ConnectionDuration = connectionDuration,
                Legs = AddLegs(segment),
                TicketNotValidAfter = orderDocSegment.TicketNotValidAfter,
                TicketNotValidBefore = orderDocSegment.TicketNotValidBefore
            });
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
                    LegDuration = GetDuration(segment?.HiddenStop, segment.ElapsedTime),
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
                    LegDuration = GetDuration(segment.HiddenStop, segment.ElapsedTime)
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

        private static IEnumerable<OrderItems> MapOrderItems(OrderCreateRequest orderCreateRequest, string orderId, GetReservationRS orderRs, List<Warning> warningList)
        {
            var orderItemList = new List<OrderItems>();
            int index = 0;

            foreach (var offer in orderCreateRequest.SelectedOffers.First().OfferItems)
            {
                orderItemList.Add(new OrderItems()
                {
                    OrderItemId = $"{orderId}-{++index}",
                    OwnerAirline = Constants.Application.OwnerAirline,
                    Services = GetServices(offer.Services, orderCreateRequest.DataList.SegmentDetails),
                    PriceDetails = GetPriceDetails(offer.OfferitemPricing.First().SalePrice, offer.OfferitemPricing.First().FareCalculationText),
                    ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                    FareComponents = string.IsNullOrWhiteSpace(offer.TicketDesigCode)
                    ? null
                    : new OrderFareComponent
                    {
                        TicketDesignatorCodes = new[] { offer.TicketDesigCode }
                    }
                });
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

        private static IEnumerable<Price> GetPriceDetails(List<Price> salePrice, string farecalculationtext)
        {
            return salePrice.Select(salePrice =>
                new Price()
                {
                    Amount = salePrice.Amount,
                    PriceComponentType = salePrice.PriceComponentType,
                    BaseCurrencyAmount = salePrice?.BaseCurrencyAmount,
                    TaxBreakup = salePrice?.TaxBreakup,
                    FareCalculation = farecalculationtext,
                    EquivalentTaxBreakup = salePrice?.EquivalentTaxBreakup
                });
        }

        private static IEnumerable<OrderService> GetServices(IEnumerable<OfferService> services, IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.Segment> segmentDetails)
        {
            return services.SelectMany(x => x.FlightSegmentsRefIds).Select((fseg, index) =>
            {
                var service = services.First(x => x.FlightSegmentsRefIds.Contains(fseg));

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
                    SegmentRefIds = new[] { fseg },
                    ServiceBeneficiaries = service.ServiceBeneficiaries,
                    ServiceDefinitionRefIds = new[] { segmentDetails.FirstOrDefault(seg => seg.SegmentId == fseg).ServiceDefinitionsRefID }
                };
            });
        }

        private static DateTime MapUtcDateTime(DateTime creationDateTimeStamp)
        {
            var cstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(creationDateTimeStamp, cstTimeZone);
        }
    }
}
