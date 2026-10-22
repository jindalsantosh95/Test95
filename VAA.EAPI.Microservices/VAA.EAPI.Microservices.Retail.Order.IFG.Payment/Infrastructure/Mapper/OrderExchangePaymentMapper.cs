using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Mapper
{
    public class OrderExchangePaymentMapper
    {
        public static PaymentAuthorizeRequest MapOrderExchange(AuthorizePaymentRequest request)
        {
            return new PaymentAuthorizeRequest
            {
                BasicInformation = new()
                {
                    Channel = Constants.Application.Channel,
                    CountryCode = request.Pos.CountryCode,
                    AgentCode = request.Parties.Participants.Where(x => x.PartyType == Participant.PartyTypeEnum.Seller).Select(o => o.AgencyID).First(),
                    NonTicketInd = true,
                    ClientID = Constants.Application.IfgClientID,
                },
                FormOfPayment = MapFormOfPayments(request),
                Orders = MapOrders(request),
                TrackID = request.TrackId
            };
        }

        private static IEnumerable<FormOfPayment> MapFormOfPayments(AuthorizePaymentRequest request)
        {
            return new List<FormOfPayment>
            {
                new()
                {
                    Amount = int.Parse(request.PaymentDetails.First().Amount.MonetaryAmount),
                    Currency = request.PaymentDetails.First().Amount.BaseCurrency,
                    Type =  (request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ? Constants.Application.EasyPayFopType : request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? $"{Constants.Application.CardType}{request.PaymentDetails.First().CardMethod.CardType}" : Constants.Application.FopType
                }
            };
        }

        private static IEnumerable<Models.Order> MapOrders(AuthorizePaymentRequest request)
        {
            var orders = request.CustomerDetails.Select(cust => new Models.Order
            {
                TransactionCode = Constants.Application.IfgTicketTransactionCode,
                ReportedFormOfPayment = MapReportedFormOfPayment(request, cust),
                FlightRecords = MapFlightRecords(cust, request),
                AdditionalInformation = new AdditionalInformation()
                {
                    PaymentInformationLine1 = request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? Constants.Application.CardPaymentInfoLine1 : Constants.Application.PaymentInfoLine1,
                    PaymentInformationLine2 = request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? Constants.PaymentTypeEnum.Card.ToString() : Constants.PaymentTypeEnum.Cash.ToString(),
                },
                AdditionalSalesInformation = MapSalesInformation(request, cust),
            });
            return orders;
        }

        private static IEnumerable<ReportedFormOfPayment> MapReportedFormOfPayment(AuthorizePaymentRequest request, Customer cust)
        {
            var amount = request.OrderItems.Where(x => x.Services.First().ServiceBeneficiaries.Contains(cust.CustId)).Select(x => x.AddCollectPrice?.Amount).FirstOrDefault();

            return new List<ReportedFormOfPayment>
            {
                new()
                {
                    Amount   = decimal.TryParse(amount?.MonetaryAmount, out var parsedAmount) ? (int)(parsedAmount * 100) : 0 ,
                    Currency = request.PaymentDetails.First().Amount.BaseCurrency + "2",
                    Type =  (request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ? Constants.Application.EasyPayFopType : request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? $"{Constants.Application.CardType}{request.PaymentDetails.First().CardMethod.CardType}" : Constants.Application.FopType
                }
            };
        }

        private static IEnumerable<FlightRecord> MapFlightRecords(VAA.EAPI.Domain.Virgin.Models.Customer.Customer customerDetails, AuthorizePaymentRequest request)
        {
            var oldTicketNumber = request.OrderHistory.SelectMany(x => x.OrderItems).SelectMany(oi => oi.AccountableDocumentRefs.Where(a => a.CustomerRefId == customerDetails.CustId)).Select(coupon => coupon.Coupons.First().TicketNumber).First();
            var coupons = request.OrderItems.Where(oi => oi.AccountableDocumentRefs != null).SelectMany(oi => oi.AccountableDocumentRefs.Where(a => a.CustomerRefId == customerDetails.CustId).SelectMany(a => a.Coupons, (a, coupon) => new { Coupon = coupon, DocumentRef = a })).ToList();
            var trackId = request.TrackIdsHistory.First(x => x.PaxRefIds.Contains(customerDetails.CustId));

            var flightRecords = new List<FlightRecord>();

            for (var i = 0; i < coupons.Count; i += 3)
            {
                var ticketCoupon = coupons[i];

                flightRecords.Add(new FlightRecord
                {
                    ConjunctionTicketIndicator = i == 0 ? false : true,
                    PassengerRecord = new PassengerRecord
                    {
                        Name = $"{customerDetails.LastName} {customerDetails.FirstName}",
                        Pnr = request.BookingReferences.RLOC + "/" + Constants.Application.SabreIndicator,
                        TypeCode = customerDetails.Ptc
                    },
                    DocumentID = new DocumentID()
                    {
                        TicketDocumentNumber = ticketCoupon.Coupon.TicketNumber,
                        TicketingAirlineCodeNumber = Constants.Application.TicketingAirLineCodeNumber,
                        TicketingModeIndicator = "/",
                        TicketCRC = Constants.Application.TicketCRC,
                    },
                    DocumentDetails = new DocumentDetails()
                    {
                        Itinerary = MapItineraryDetails(request.Segments, customerDetails, request, i + 1),
                        ExchangeDetails = new ExchangeDetails[]
                        {
                            new()
                            {
                                 OriginalTrackID = trackId.TrackId,
                                 RelatedTicketDocumentNumber = oldTicketNumber,
                                 RelatedTicketDocumentCouponNumberIdentifier = GenerateCouponIdentifier(request.OrderHistory.First().DataList.SegmentDetails.Count())
                            }
                        }
                    },
                    DateOfIssuance = DateTime.ParseExact(coupons.FirstOrDefault()?.DocumentRef.IssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                    AutomaticRepricingEngine = " ",
                    StatisticalCode = Constants.Application.StatisticalCodes
                });
            }

            return flightRecords;
        }

        private static IEnumerable<Itinerary> MapItineraryDetails(IEnumerable<Segment> segments, VAA.EAPI.Domain.Virgin.Models.Customer.Customer customerDetails, AuthorizePaymentRequest request, int segmentIndex)
        {
            var itineraries = new List<Itinerary>();
            int index = 0;
            var segmentList = segments.ToList();

            for (int i = segmentIndex; i <= segmentIndex + 3 && i <= request.Segments.Count(); i++)
            {
                var segment = request.Segments.FirstOrDefault(segId => segId.SegmentId.ToUpper().Replace("SEG", "").Equals(i.ToString()));

                itineraries.Add(new Itinerary
                {
                    OriginCode = segment.OriginAirportCode,
                    DestinationCode = segment.DestinationAirportCode,
                    BaggageAllowance = $"{MapBaggageAllowanceDetails(segment.SegmentId, customerDetails.CustId, request)}PC",
                    CouponUseIndicator = Constants.Application.CouponUseIndicator,
                    SoldPassengerCabin = GetCabinCode(segment.CabinType?.CabinName ?? ""),
                    SegmentIdentifier = (++index).ToString(),
                    Carrier = segment.MarketingCarrier,
                    StopoverCode = segment?.ConnectionOrStopOver,
                    ReservationBooking = segment.ClassOfService,
                    FareBasisTicketDesignator = segment.FareBasis,
                    FlightArrivalTime = DateTime.Parse(segment.ScheduledArrivalTime.LocalDateTime).ToString("HH:mm:ss"),
                    FlightArrivalDate = DateTime.Parse(segment.ScheduledArrivalTime.LocalDateTime).ToString("yyyyMMdd"),
                    FlightNumber = segment.MarketingFlightNumber ?? segment.OperatingFlightNumber,
                    FlightDate = DateTime.Parse(segment.ScheduledDepartureTime.LocalDateTime).ToString("yyyyMMdd"),
                    FlightTime = DateTime.Parse(segment.ScheduledDepartureTime.LocalDateTime).ToString("HH:mm:ss"),
                });

                // Identify Arank Segments
                if (i + 1 <= request.Segments.Count())
                {
                    var next = segmentList[i];

                    if (!segment.DestinationAirportCode.Is(next.OriginAirportCode))
                    {
                        // Map Arank Segments Coupon Use Indicator
                        itineraries.Add(new Itinerary
                        {
                            OriginCode = segment.DestinationAirportCode,
                            DestinationCode = next.OriginAirportCode,
                            CouponUseIndicator = Constants.Application.ArankSegmentCouponUseIndicator,
                            SegmentIdentifier = (++index).ToString()
                        });
                    }
                }
            }

            return itineraries;
        }

        private static int MapBaggageAllowanceDetails(string segmentRefId, string passengerRefId, AuthorizePaymentRequest request)
        {
            if (request?.OrderItems == null || request.DataList?.BaggageAllowances == null) return 0;

            int checkedBaggageCount = 0;

            foreach (var orderItem in request.OrderItems)
            {
                foreach (var service in orderItem.Services)
                {
                    if (service.SegmentRefIds?.Contains(segmentRefId) == true && service.ServiceBeneficiaries?.Contains(passengerRefId) == true)
                    {
                        foreach (var bagRefId in service.BaggageRefIds ?? Enumerable.Empty<string>())
                        {
                            var baggage = request.DataList.BaggageAllowances.FirstOrDefault(b => b.BaggageAllowanceId == bagRefId && b.BaggageAllowanceTypeCode.Is("Checked"));

                            if (baggage != null)
                            {
                                checkedBaggageCount += baggage.FreeBaggageAllowance?.AllowedBagCnt ?? 0;
                            }
                        }
                    }
                }
            }

            return checkedBaggageCount;
        }

        private static string GetCabinCode(string CabinName)
        {
            string resultstring = "";

            switch (CabinName?.Trim().ToLower())
            {
                case "upper class":
                case "business":
                    return "C";

                case "premium economy":
                    return "W";

                case "economy":
                    return "Y";

                default:
                    return resultstring;
            }
        }

        private static string GenerateCouponIdentifier(int count)
        {
            var result = "";
            for (int i = 1; i <= count; i++)
            {
                if (i <= count)
                    result += i;
            }

            for (int i = result.Length; i < 4; i++)
            {
                result += "0";
            }

            return result;
        }

        private static IEnumerable<AdditionalSalesInformation> MapSalesInformation(AuthorizePaymentRequest request, VAA.EAPI.Domain.Virgin.Models.Customer.Customer cust)
        {
            var orderItems = request.OrderItems.Where(x => x.Services.FirstOrDefault()?.ServiceBeneficiaries != null && x.Services.FirstOrDefault()?.ServiceBeneficiaries.Contains(cust.CustId) == true);

            // Get base fare amount
            var basePriceComponent = orderItems.FirstOrDefault()?.PriceDetails?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum);
            string fareAmount = null;
            if (decimal.TryParse(basePriceComponent?.Amount?.MonetaryAmount, out var parsedBaseFare))
            {
                var currency = basePriceComponent.Amount.BaseCurrency ?? "";
                fareAmount = $"{currency}{parsedBaseFare.ToString(CultureInfo.InvariantCulture)}";
            }

            // Get total tax amount
            var taxComponent = orderItems.FirstOrDefault()?.PriceDetails?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BasePriceEnum);
            string taxAmount = null;
            if (decimal.TryParse(taxComponent?.Amount?.MonetaryAmount, out var parsedTax))
            {
                var currency = taxComponent.Amount.BaseCurrency ?? "";
                taxAmount = $"{currency}{parsedTax.ToString(CultureInfo.InvariantCulture)}";
            }

            var accountableDocuments = request.OrderHistory.SelectMany(x => x.OrderItems).SelectMany(oi => oi.AccountableDocumentRefs.Where(a => a.CustomerRefId == cust.CustId).ToList());

            var originalIssueDateRaw = accountableDocuments.FirstOrDefault()?.IssueDate;

            string formattedIssueDate = null;
            if (!string.IsNullOrEmpty(originalIssueDateRaw) && DateTime.TryParseExact(originalIssueDateRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                formattedIssueDate = parsedDate.ToString("yyyyMMdd");
            }

            var additionalSalesInformation = new List<AdditionalSalesInformation>
            {
                new AdditionalSalesInformation
                {
                        Fare = fareAmount,
                        Total = taxAmount,
                        FareCalculationModeIndicator = Constants.Application.FareCalculationModeIndicator,
                        FareCalculationPricingIndicator = Constants.Application.FareCalculationPricingIndicator,
                        FareCalculationArea1 = basePriceComponent.FareCalculation.Length > 87 ? basePriceComponent.FareCalculation.Substring(0,87) : basePriceComponent.FareCalculation,
                        FareCalulationArea2 = basePriceComponent.FareCalculation.Length > 87 ? basePriceComponent.FareCalculation.Substring(87) : null,
                        OriginalTicketDocumentNumber = accountableDocuments.FirstOrDefault()?.DocumentNumber,
                        OriginalIssueDate = formattedIssueDate,
                        OriginalIssueAgentNumericCode = request.Parties.Participants.Where(x => x.PartyType == Participant.PartyTypeEnum.Seller).Select(o => o.AgencyID).First(),
                        OriginalIssueLocationCityCode = request.Pos.CityCode == null ? Constants.Application.NameCorrectionCityCode : request.Pos.CityCode,
                     }
            };

            return additionalSalesInformation;
        }
    }
}
