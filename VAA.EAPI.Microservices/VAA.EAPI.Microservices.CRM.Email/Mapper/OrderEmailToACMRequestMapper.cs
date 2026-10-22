using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Microservices.CRM.Email.Domain;
using static VAA.EAPI.Domain.Virgin.Models.Common.Product;
using static VAA.EAPI.Microservices.CRM.Email.Domain.EmailTemplate;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public static class OrderEmailToACMRequestMapper
    {
        public static string MapRequest(SendOrderEmailRequest request)
        {
            var orderDocument = request.OrderDocument;

            var email = orderDocument.Parties?.Participants?.FirstOrDefault()?.RequestAgentType is "IATA" or "TIDS"
                        ? orderDocument.Parties?.Participants?.FirstOrDefault()?.Contacts?.Email
                        : orderDocument.DataList?.CustomerDetails?.FirstOrDefault()?.Contacts?.Email;

            var ctx = Map(request, email);

            var serializer = new XmlSerializer(ctx.GetType());

            XElement ctxElement;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, ctx);
                stream.Position = 0;
                ctxElement = XElement.Load(stream);
            }

            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace urn = "urn:nms:rtEvent";

            XElement soapuirequest = new(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "urn", urn),
                new XElement(soapenv + "Header"),
                new XElement(soapenv + "Body",
                    new XElement(urn + "PushEvent",
                        new XElement(urn + "sessiontoken", ConfigReader.Get<bool>(Constants.Environment.EnableEmailMSAuthentication) ? string.Empty : ConfigReader.Get(Constants.Environment.ACMSessionToken)),
                        new XElement(urn + "domEvent",
                            new XElement("rtEvent",
                                new XAttribute("type", ConfigReader.Get(Constants.Environment.BookingAcknowledgmentTemplate)),
                                new XAttribute("wishedChannel", ConfigReader.Get(Constants.Environment.ACMCommunicationChannel)),
                                new XAttribute("email", email),
                                new XElement("ctx", ctxElement.FirstNode)
                                )
                            )
                        )
                    )
                );

            return soapuirequest.ToString();
        }

        public static ctx Map(SendOrderEmailRequest request, string email)
        {
            var order = request.OrderDocument;

            var customers = order.DataList?.CustomerDetails ?? new List<Customer>();
            var primaryCustomer = customers.FirstOrDefault();

            var result = new ctx
            {
                SendMessage = new SendMessage
                {
                    ProcessingMetadata = new ProcessingMetadata
                    {
                        EmailAddress = email,
                        Channel = "NDC",
                        Template = "BKGACK"
                    },
                    MessageData = new MessageData
                    {
                        Wrapper = new Wrapper
                        {
                            document = new List<document>()
                        },
                    }
                }
            };

            // Building one document per customer 
            foreach (var cust in order?.DataList?.CustomerDetails)
            {
                var doc = new document
                {
                    request = new request
                    {
                        template_id = "247382",
                        email_address = primaryCustomer?.Contacts?.Email,
                        test = "1",
                        CONTENT_DATA = new CONTENT_DATA()
                    }
                };

                var orderItem = GetFlightOrderItemForPtc(order, cust?.Ptc);
                var content = doc.request.CONTENT_DATA;
                var passengerName = $"{cust?.FirstName ?? string.Empty} {cust?.LastName ?? string.Empty}";
                var seatEfee = BuildSeatEFeesForCustomer(request, cust);

                if (seatEfee != null && seatEfee.Any())
                {
                    content.EFEE ??= new List<EFEE>();
                    content.EFEE.AddRange(seatEfee);
                }

                content.CTRY_CD = order.PointOfSale?.CountryCode;
                content.NBR_PSNGRS = "1";
                content.PSNGR_NM = passengerName;
                content.ITINERARY = BuildItineraryPerCustomer(order, cust);
                content.FARE = BuildFareForOrderItem(orderItem, request);
                content.AWARD_MILEAGE = string.Empty;
                content.TAXES_CHARGES = BuildTaxesChargesForOrderItem(orderItem, order);
                content.TICKET_DTL = BuildTicketDetailForCustomer(order, cust.CustId);
                content.PTC_ATP_PAX_CD = cust?.Ptc;
                content.PTC_DL_PAX_TXT = cust?.ItaPtc?.Substring(0, 1);

                result.SendMessage.MessageData.Wrapper.document.Add(doc);
            }

            return result;
        }

        private static ITINERARY BuildItineraryPerCustomer(OrderDocument order, Customer cust)
        {
            var ticketDetail = BuildTicketDetailForCustomer(order, cust.CustId);

            var itin = new ITINERARY
            {
                DL_REC_LOC = order.BookingReference?.RLOC,
                TCKT_NBR = ticketDetail?.TCKT_NBR,
                NEST = new List<NEST>(),
                PSNGR_INFO = BuildPsnrInfoForCustomer(order, cust)
            };

            var segments = order.DataList?.SegmentDetails?.ToList();
            if (segments == null || !segments.Any())
                return itin;

            segments = segments
                .Where(s => s?.ScheduledDepartureTime?.LocalDateTime != null)
                .OrderBy(s => DateTime.Parse(s.ScheduledDepartureTime.LocalDateTime))
                .ToList();

            var firstSeg = segments.FirstOrDefault();
            var lastSeg = segments.LastOrDefault();

            var origin = firstSeg?.OriginAirportCode;
            var finalDestination = lastSeg?.DestinationAirportCode;

            itin.ITINERARY_ORIGIN = origin;
            itin.ITINERARY_DESTINATION = finalDestination;

            itin.NEXT_TRIP_ORIGIN = origin;

            var visited = new List<string>();
            string furthestPoint = origin;

            foreach (var seg in segments)
            {
                var dest = seg?.DestinationAirportCode;

                if (string.IsNullOrEmpty(dest))
                    continue;

                if (dest.Equals(origin) && visited.Count > 0)
                    break;

                if (visited.Contains(dest))
                    break;

                visited.Add(dest);
                furthestPoint = dest;
            }

            itin.NEXT_TRIP_DESTINATION = furthestPoint;

            var isRoundTrip = !string.IsNullOrEmpty(origin)
                              && !string.IsNullOrEmpty(finalDestination)
                              && origin.Equals(finalDestination);

            if (isRoundTrip)
            {
                itin.ITINERARY_ROUNDTRIP = origin;
            }

            foreach (var seg in segments)
            {
                var hasLayover = seg.Legs != null && seg.Legs.Any(leg => !string.IsNullOrEmpty(leg.StopOverDuration));

                var segElement = new SEGMENT
                {
                    SEGMENT_NO = seg.SegmentId,
                    DEPART_DAY = DateTime.Parse(seg?.ScheduledDepartureTime?.LocalDateTime).ToString("ddd").ToUpper(),
                    DEPART_DATE = DateTime.Parse(seg?.ScheduledDepartureTime?.LocalDateTime).ToString("ddMMM").ToUpper(),
                    DEPART_TIME = DateTime.Parse(seg?.ScheduledDepartureTime?.LocalDateTime).ToString("h:mmtt").ToLower(),
                    DEPART_CITY = seg.OriginAirportCode,
                    AIRLINE_CD = seg.OperatingCarrier ?? "VS",
                    AIRLINE_NAME = seg.OperatingCarrier ?? "Virgin Atlantic",
                    FLIGHT_NBR = seg.MarketingFlightNumber,
                    COUPON_STATUS = seg.Status?.Code,
                    MEALS_OTHER = new List<MEALS_OTHER>(),
                    BKNG_CLS = seg.ClassOfService,
                    CABIN = seg.CabinType?.CabinName,
                    ARRIVE_DAY = DateTime.Parse(seg.ScheduledArrivalTime.LocalDateTime).ToString("ddd").ToUpper(),
                    ARRIVE_DATE = DateTime.Parse(seg.ScheduledArrivalTime.LocalDateTime).ToString("ddMMM").ToUpper(),
                    ARRIVE_TIME = DateTime.Parse(seg.ScheduledArrivalTime?.LocalDateTime).ToString("h:mmtt").ToLower(),
                    ARRIVE_CITY = seg.DestinationAirportCode,
                    LAYOVER_IND = hasLayover ? "Y" : string.Empty,
                    DepartureAirport = seg.OriginAirportCode,
                    ArrivalAirport = seg.DestinationAirportCode,
                    TERMINAL_INFO = new TERMINAL_INFO
                    {
                        FlightNumber = seg.MarketingFlightNumber,
                        FlightDesig = seg.MarketingCarrier,
                        FlightDate = DateTime.Parse(seg.ScheduledDepartureTime.LocalDateTime).ToString("dd/MM/yyyy"),
                        DepartureAirport = seg.OriginAirportCode,
                        ArrivalAirport = seg.DestinationAirportCode,
                        ClassCode = seg.ClassOfService,
                        OperatedBy = seg.OperatingCarrier ?? seg.MarketingCarrier,
                    },
                };

                itin.NEST.Add(new NEST
                {
                    DEPART_DAY = segElement.DEPART_DAY,
                    DEPART_DATE = segElement.DEPART_DATE,
                    SEGMENT = segElement
                });
            }

            return itin;
        }

        private static PSNGR_INFO BuildPsnrInfoForCustomer(OrderDocument order, Customer cust)
        {
            var ps = new PSNGR_INFO
            {
                FULL_NM = $"{cust?.FirstName ?? ""} {cust?.LastName ?? ""}".Trim(),
                LNAME = cust?.LastName,
                FNAME = cust?.FirstName,
                SEAT_ASGMT = new List<SEAT_ASGMT>(),
                SKYMILES_NBR = string.Empty,
            };

            var segmentIds = order.OrderItems
               .SelectMany(o => o.Services)
               .Where(s =>
                   s.Product?.ProductType == ProductTypes.FlightEnum &&
                   s.ServiceBeneficiaries != null &&
                   s.ServiceBeneficiaries.Contains(cust.CustId))
               .SelectMany(s => s.SegmentRefIds)
               .Distinct()
               .ToList();

            var segments = order.DataList.SegmentDetails
               .Where(s => segmentIds.Contains(s.SegmentId));

            foreach (var seg in segments)
            {
                if (string.Equals(cust.Ptc, "INF", StringComparison.OrdinalIgnoreCase))
                    continue;

                var seat = order.OrderItems
                    .SelectMany(oi => oi.Services)
                    .Where(s =>
                        s.Product?.ProductType == ProductTypes.SeatEnum &&
                        s.ServiceBeneficiaries != null &&
                        s.ServiceBeneficiaries.Contains(cust.CustId) &&
                        s.SegmentRefIds != null &&
                        s.SegmentRefIds.Contains(seg.SegmentId))
                    .Select(s => s.CurrentSeatAssignment)
                    .FirstOrDefault();

                ps.SEAT_ASGMT.Add(new SEAT_ASGMT
                {
                    FLIGHT_NBR = seg.MarketingFlightNumber,
                    SEAT = string.IsNullOrEmpty(seat) ? "**" : seat
                });
            }

            return ps;
        }

        private static FARE BuildFareForOrderItem(OrderItems orderItem, SendOrderEmailRequest request)
        {
            var baseFarePd = orderItem?.PriceDetails?.FirstOrDefault(pd => pd.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BaseFareEnum);
            var totalTaxPd = orderItem?.PriceDetails?.FirstOrDefault(pd => pd.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TaxEnum);
            var basePricePd = orderItem?.PriceDetails?.FirstOrDefault(pd => pd.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum);
            var paymentdetails = orderItem?.PaymentDetails.FirstOrDefault();

            var baseAmt = baseFarePd?.Amount?.MonetaryAmount;
            var taxAmt = totalTaxPd?.Amount?.MonetaryAmount;
            var totl = basePricePd?.Amount?.MonetaryAmount;
            if (decimal.TryParse(baseAmt, out decimal baseValue) && decimal.TryParse(taxAmt, out decimal taxValue))
            {
                decimal sum = baseValue + taxValue;
                totl = sum.ToString("F2");
            }

            return new FARE
            {
                FARE_CALC1 = basePricePd?.FareCalculation ?? string.Empty,
                BASE_FARE_AMT = baseFarePd?.Amount?.MonetaryAmount ?? string.Empty,
                BASE_FARE_CUR_CD = baseFarePd?.Amount?.BaseCurrency ?? string.Empty,
                EQUIV_FARE_PAID_AMT = basePricePd?.Amount?.MonetaryAmount ?? string.Empty,
                EQUIV_FARE_CUR_CD = basePricePd?.Amount?.BaseCurrency ?? string.Empty,
                TAX_AMT = totalTaxPd?.Amount?.MonetaryAmount ?? string.Empty,
                TOTL_FARE_AMT = basePricePd?.Amount?.MonetaryAmount ?? string.Empty,
                TOTL_FARE_CUR_CD = basePricePd?.Amount?.BaseCurrency ?? string.Empty,
                FOP = request.RequestType == Constants.RequestType.EvenExchange ? "-" : paymentdetails.FormOfPayments.ToString() == "CashEnum" ? "CA" : paymentdetails.FormOfPayments.ToString() == "EasyPay" ? "EP" : $"{paymentdetails?.CardMethod?.CardBrandCode}{paymentdetails?.CardMethod?.CardNumber}" ?? string.Empty,
                CC_NBR = paymentdetails?.CardMethod?.CardNumber ?? string.Empty,
                VENDOR = request.RequestType == Constants.RequestType.EvenExchange ? "-" : paymentdetails.FormOfPayments.ToString() == "CashEnum" ? "CA" : paymentdetails.FormOfPayments.ToString() == "EasyPay" ? "EP" : paymentdetails?.CardMethod?.CardBrandCode ?? string.Empty,
                PAID_WITH_ONLINE = "N",
            };
        }

        private static TAXES_CHARGES BuildTaxesChargesForOrderItem(OrderItems orderItem, OrderDocument order)
        {
            var taxes = new TAXES_CHARGES
            {
                CHARGE = new List<CHARGE>(),
                TAX = new List<TAX>(),
                TOTL_TAXES = new TOTL_TAXES(),
                TOTL_TAXES_CHARGES = new TOTL_TAXES_CHARGES()
            };

            if (orderItem?.PriceDetails == null) return taxes;

            var taxPd = orderItem.PriceDetails?.FirstOrDefault(pd => pd.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum || pd.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TaxEnum);

            if (taxPd?.TaxBreakup != null)
            {
                foreach (var tb in taxPd.TaxBreakup)
                {
                    if (string.Equals(tb.Code, "YQ") || string.Equals(tb.Code, "YR"))
                    {
                        taxes.CHARGE.Add(new CHARGE
                        {
                            CHARGE_CD = tb.Code,
                            CHARGE_DESC = tb.Name,
                            CHARGE_AMT = tb.Amount?.MonetaryAmount,
                            CHARGE_CURR_CD = tb.Amount?.BaseCurrency
                        });
                    }
                    else
                    {
                        taxes.TAX.Add(new TAX
                        {
                            TAX_CD = tb.Code,
                            TAX_DESC = tb.Name,
                            TAX_AMT = tb.Amount?.MonetaryAmount,
                            TAX_CURR_CD = tb.Amount?.BaseCurrency
                        });
                    }
                }

                taxes.TOTL_TAXES = new TOTL_TAXES
                {
                    TOTL_AMT = taxPd.Amount?.MonetaryAmount,
                    TOTL_CURR_CD = taxPd.Amount?.BaseCurrency
                };

                taxes.TOTL_TAXES_CHARGES = new TOTL_TAXES_CHARGES
                {
                    TOTL_AMT = taxPd.Amount?.MonetaryAmount,
                    TOTL_CURR_CD = taxPd.Amount?.BaseCurrency
                };
            }

            return taxes;
        }

        private static TICKET_DTL BuildTicketDetailForCustomer(OrderDocument order, string custId)
        {
            var docRef = order.OrderItems?
                .SelectMany(oi => oi.AccountableDocumentRefs)
                .FirstOrDefault(d => d.CustomerRefId == custId);

            if (docRef == null) { return null; }

            return new TICKET_DTL
            {
                TCKT_NBR = docRef.DocumentNumber,
            };
        }

        private static IEnumerable<EFEE> BuildSeatEFeesForCustomer(SendOrderEmailRequest request, Customer cust)
        {
            var order = request.OrderDocument;

            return order.OrderItems
                .Where(oi =>
                    oi.Services != null &&
                    oi.Services.Any(s =>
                        s.Product?.ProductType == ProductTypes.SeatEnum &&
                        s.ServiceBeneficiaries?.Contains(cust.CustId) == true) &&
                    oi.PriceDetails != null &&
                    oi.PriceDetails.Any() &&
                    oi.PriceDetails.Any(p =>
                        p.PriceComponentType ==
                        EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum &&
                        p.Amount != null &&
                        decimal.TryParse(p.Amount.MonetaryAmount, out var seatAmount) &&
                        seatAmount > 0))
                .Select(oi =>
                {
                    var seatService = oi.Services.First(s =>
                        s.Product?.ProductType == ProductTypes.SeatEnum &&
                        s.ServiceBeneficiaries?.Contains(cust.CustId) == true);

                    var basePrice = oi.PriceDetails.First(p =>
                        p.PriceComponentType ==
                        EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum);

                    var docRef = oi.AccountableDocumentRefs?
                        .FirstOrDefault(d => d.CustomerRefId == cust.CustId);

                    var payment = oi.PaymentDetails?.FirstOrDefault();

                    return new EFEE
                    {
                        PSGR_NM = $"{cust.FirstName} {cust.LastName}",
                        ISSUE_DTE = docRef?.IssueDate,

                        TRANSACTION = new TRANSACTION
                        {
                            TRANS_DESC = seatService.Product?.ProductName,
                            TRANS_AMT = basePrice.Amount.MonetaryAmount,
                            TRANS_CUR_CD = basePrice.Amount.BaseCurrency
                        },

                        TOTAL_AMT = basePrice.Amount.MonetaryAmount,
                        TOTAL_CUR_CD = basePrice.Amount.BaseCurrency,

                        FOP = $"{payment?.CardMethod?.CardBrandCode}{payment?.CardMethod?.MaskedCardNumber}",
                        FOP_VENDOR = payment?.CardMethod?.CardBrandCode,
                        FOP_CC_NBR = payment?.CardMethod?.MaskedCardNumber,

                        ROUTING_FEE =
                            $"{order.DataList.OriginDestinationAssociations.First().OriginDestination.OriginAirport} " +
                            $"VS {order.DataList.OriginDestinationAssociations.Last().OriginDestination.DestinationAirport}",

                        EMD_TYPE = request.RequestType == Constants.RequestType.PostSale ? "A" : "S"
                    };
                });
        }

        private static OrderItems GetFlightOrderItemForPtc(OrderDocument order, string ptc)
        {
            foreach (var oi in order.OrderItems)
            {
                if (oi.Services == null)
                    continue;

                foreach (var svc in oi.Services)
                {
                    if (svc.Product?.ProductType != ProductTypes.FlightEnum)
                        continue;

                    if (svc.ServiceBeneficiaries == null)
                        continue;

                    foreach (var custId in svc.ServiceBeneficiaries)
                    {
                        var cust = order.DataList.CustomerDetails
                            .First(c => c.CustId == custId);

                        if (cust.Ptc == ptc)
                            return oi;
                    }
                }
            }

            return null;
        }

    }
}
