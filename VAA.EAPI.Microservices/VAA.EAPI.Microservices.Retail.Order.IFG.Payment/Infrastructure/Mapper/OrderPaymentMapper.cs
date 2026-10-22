using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Mapper.Common;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Mapper
{
    public class OrderPaymentMapper
    {
        public static PaymentAuthorizeRequest OrderPaymentRequest(AuthorizePaymentRequest request)
        {
            return new PaymentAuthorizeRequest
            {
                BasicInformation = new()
                {
                    Channel = Constants.Application.Channel,
                    CountryCode = request.Pos.CountryCode,
                    AgentCode = request.Parties.Participants.First().AgencyID,
                    NonTicketInd = true,
                    ClientID = Constants.Application.IfgClientID,
                    TASF = false
                },
                FormOfPayment = MapFormOfPayments(request.PaymentDetails, request.Parties),
                Orders = MapOrders(request),
                TrackID = request.TrackId,
            };
        }

        private static IEnumerable<FormOfPayment> MapFormOfPayments(IEnumerable<VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain.Payment> payments, Parties parties)
        {
            var paymentInfo = payments.First();
            bool ifgAdditionalFieldsEnabled = bool.Parse(ConfigReader.Get(Constants.Environment.IfgAdditionalFieldsEnabled));

            return
            [
                paymentInfo.FormOfPayments ==  EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay ?
                new()
                {
                    Amount   = int.Parse(paymentInfo.Amount.MonetaryAmount),
                    Currency = paymentInfo.Amount.BaseCurrency,
                    Type     = Constants.Application.EasyPayFopType
                } : paymentInfo.FormOfPayments == Domain.Payment.FormOfPayment.CardEnum
                ? new FormOfPayment()
                {
                    Amount   = int.Parse(paymentInfo.Amount.MonetaryAmount),
                    Currency = paymentInfo.Amount.BaseCurrency,
                    Type     = $"{Constants.Application.CardType}{paymentInfo.CardMethod.CardType}",
                    Payer = new()
                    {
                        Name = $"{paymentInfo.Payer.LastName} {paymentInfo.Payer.FirstName}",
                        PhoneNumber =$"{paymentInfo.Payer.PayerContacts.CountryDailingCode}-{paymentInfo.Payer.PayerContacts.Phone}",
                        Email = paymentInfo.Payer.PayerContacts.Email,
                    },
                    CardSecurityCode = paymentInfo.CardMethod.Cvv,
                    ExpiryDate = string.Join("", paymentInfo.CardMethod.ExpiryDate.Split('/')),
                    AccountNumber = paymentInfo.CardMethod.CardNumber,
                    Address = new ()
                    {
                        HouseNumberOrName = paymentInfo.Payer.PayerAddress.AddressLine1,
                        Street = paymentInfo.Payer.PayerAddress.AddressLine2,
                        City = paymentInfo.Payer.PayerAddress.City,
                        Country = paymentInfo.Payer.PayerAddress.Country,
                        CountryCode = paymentInfo.Payer.PayerAddress.Country,
                        PostalCode = paymentInfo.Payer.PayerAddress.PostalCode
                    },
                    MerchantDefinedInformation = ifgAdditionalFieldsEnabled ? HelperMethodMapper.GetMerchantDefinedInformation(parties) : null,
                    MotoPayment = paymentInfo?.PaymentCardAuthentication == null
                } : new()
                {
                    Amount   = int.Parse(paymentInfo.Amount.MonetaryAmount),
                    Currency = paymentInfo.Amount.BaseCurrency,
                    Type     = Constants.Application.FopType
                }


            ];
        }

        private static IEnumerable<Models.Order> MapOrders(AuthorizePaymentRequest request)
        {
            var orders = new List<Models.Order>();

            foreach (var cust in request.CustomerDetails)
            {
                orders.Add(new Models.Order
                {
                    TransactionCode = request.ProductType.Equals(ProductType.Ticket) ? Constants.Application.IfgTicketTransactionCode : Constants.Application.IfgSeatransactionCode,
                    ReportedFormOfPayment = MapReportedFormOfPayments(request, cust),
                    FlightRecords = MapFlightRecords(cust, request),
                    AdditionalInformation = new AdditionalInformation
                    {
                        PaymentInformationLine1 = request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum ? Constants.Application.PaymentInfoLine1 : Constants.Application.CardPaymentInfoLine1,
                        PaymentInformationLine2 = request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum ? Constants.PaymentTypeEnum.Cash.ToString() : Constants.PaymentTypeEnum.Card.ToString()
                    },
                    AdditionalSalesInformation = MapSalesInformation(request, cust),
                    MonetaryAmounts = MapMonetaryAmounts(request, cust)
                });
            }

            return orders;
        }

        private static IEnumerable<ReportedFormOfPayment> MapReportedFormOfPayments(AuthorizePaymentRequest request, Customer cust)
        {
            var basePrice = request.OrderItems.Where(x => x.Services.First().ServiceBeneficiaries.Contains(cust.CustId)).Select(x => x.PriceDetails.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BasePriceEnum)).FirstOrDefault();

            return new List<ReportedFormOfPayment>
            {
                new()
                {
                    Amount   = int.Parse(decimal.TryParse(basePrice.Amount?.MonetaryAmount, out var parsedAmount) ? ((int)(parsedAmount * 100)).ToString(): null),
                    Currency = basePrice.Amount.BaseCurrency+ "2",
                    Type =  (request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ? Constants.Application.EasyPayFopType : request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? $"{Constants.Application.CardType}{request.PaymentDetails.First().CardMethod.CardType}" : Constants.Application.FopType
                }
            };
        }

        private static MonetaryAmounts MapMonetaryAmounts(AuthorizePaymentRequest request, Customer cust)
        {
            var taxFeeRecords = new List<TaxFeeRecord>();

            var orderItems = request.OrderItems.Where(x => x.Services.FirstOrDefault()?.ServiceBeneficiaries.Contains(cust.CustId) == true);

            foreach (var orderItem in orderItems)
            {
                var totalTaxComponent = orderItem.PriceDetails.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.TotalTaxEnum);

                if (totalTaxComponent?.TaxBreakup == null) continue;

                foreach (var tax in totalTaxComponent.TaxBreakup)
                {
                    if (decimal.TryParse(tax.Amount?.MonetaryAmount, out var parsedAmount))
                    {
                        taxFeeRecords.Add(new TaxFeeRecord
                        {
                            type = tax.Code,
                            amount = (int)(parsedAmount * 100)
                        });
                    }
                }
            }

            return new MonetaryAmounts
            {
                TaxFeeRecords = taxFeeRecords
            };
        }

        private static IEnumerable<AdditionalSalesInformation> MapSalesInformation(AuthorizePaymentRequest request, Customer cust)
        {
            var orderItems = request.OrderItems.Where(x => x.Services.FirstOrDefault()?.ServiceBeneficiaries.Contains(cust.CustId) == true);

            // Get base fare amount

            var basePriceComponent = orderItems.FirstOrDefault()?.PriceDetails?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum);
            string fareAmount = null;
            if (decimal.TryParse(basePriceComponent?.Amount?.MonetaryAmount, out var parsedBaseFare))
            {
                var currency = basePriceComponent.Amount.BaseCurrency ?? "";
                fareAmount = $"{currency}{parsedBaseFare.ToString(CultureInfo.InvariantCulture)}";
            }

            // Get total tax amount
            var taxComponent = orderItems.FirstOrDefault()?.PriceDetails.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.TotalTaxEnum);
            string taxAmount = null;
            if (decimal.TryParse(taxComponent?.Amount?.MonetaryAmount, out var parsedTax))
            {
                var currency = taxComponent.Amount.BaseCurrency ?? "";
                taxAmount = $"{currency}{parsedTax.ToString(CultureInfo.InvariantCulture)}";
            }

            var additionalSalesInformation = new List<AdditionalSalesInformation>
            {
                 new AdditionalSalesInformation
                 {
                        Fare = fareAmount,
                        Total = taxAmount,
                        FareCalculationModeIndicator = Constants.Application.FareCalculationModeIndicator,
                        FareCalculationArea1 = basePriceComponent.FareCalculation.Length > 87 ? basePriceComponent.FareCalculation.Substring(0,87) : basePriceComponent.FareCalculation,
                        FareCalulationArea2 = basePriceComponent.FareCalculation.Length > 87 ? basePriceComponent.FareCalculation.Substring(87) : null,
                        FareCalculationPricingIndicator = Constants.Application.FareCalculationPricingIndicator
                 }
            };

            return additionalSalesInformation;
        }

        private static IEnumerable<FlightRecord> MapFlightRecords(VAA.EAPI.Domain.Virgin.Models.Customer.Customer customerDetails, AuthorizePaymentRequest request)
        {
            IEnumerable<AccountableDocumentRef> accountableAccountableDocumentRefDoc = null;
            if(request.IsChangeReshopHoldBooking)
            {
                accountableAccountableDocumentRefDoc = request?.OrderItems?.SelectMany(x => x.AccountableDocumentRefs).Where(a => a.CustomerRefId == customerDetails.CustId); 
            }

            var coupons = !request.IsChangeReshopHoldBooking ? (request.AccountableDocumentRef.Where(accountableDoc => accountableDoc.CustomerRefId == customerDetails.CustId).SelectMany(x => x.Coupons).ToList()) : accountableAccountableDocumentRefDoc.SelectMany(c => c.Coupons).ToList(); 
            var flightRecords = new List<FlightRecord>();

            int conjuctiveCouponCount = 4;
            bool isConjunction = false;

            for (var i = 0; i < coupons.Count; i += conjuctiveCouponCount)
            {
                var couponTicket = coupons.Skip(i).Take(conjuctiveCouponCount);

                var flightRecord = new FlightRecord
                {
                    ConjunctionTicketIndicator = isConjunction,
                    PassengerRecord = new PassengerRecord
                    {
                        Name = $"{customerDetails.LastName} {customerDetails.FirstName}",
                        Pnr = request.BookingReferences.RLOC + "/" + Constants.Application.SabreIndicator,
                        TypeCode = customerDetails.Ptc
                    },
                    DocumentID = new DocumentID()
                    {
                        TicketDocumentNumber = couponTicket.Select(c => c.TicketNumber).First(),
                        TicketingAirlineCodeNumber = Constants.Application.TicketingAirLineCodeNumber,
                        TicketCRC = Constants.Application.TicketCRC,
                        TicketingModeIndicator = "/"
                    },
                    DocumentDetails = new DocumentDetails()
                    {
                        Itinerary = MapItineraryDetails(request.Segments, customerDetails, request, i + 1),
                        EmdDetails = request.ProductType.Equals(ProductType.Seat) ? new[]
                        {
                            new EmdDetails()
                            {
                                CouponNumber = Constants.Application.SeatCouponNumber,
                                CouponValue = int.Parse(request.PaymentDetails.First().Amount.MonetaryAmount),
                                RelatedTicketDocumentNumber = request.AccountableDocumentRef.First(accountableDoc => accountableDoc.CustomerRefId == customerDetails.CustId).DocumentNumber,
                                RelatedCouponNumber = Constants.Application.SeatCouponNumber,
                                ReasonForIssuanceSubCode = "",
                                FeeOwnerAirlineDesignator  = Constants.Application.AirlineDesignator,
                                NumberOfServices = Constants.Application.SeatCouponServices,
                            }
                        } : null,
                    },
                    DateOfIssuance = !request.IsChangeReshopHoldBooking ? DateTime.ParseExact(request.AccountableDocumentRef.First().IssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyyMMdd") : DateTime.ParseExact(accountableAccountableDocumentRefDoc.First().IssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                    AutomaticRepricingEngine = " ",
                    StatisticalCode = Constants.Application.StatisticalCodes
                };

                flightRecords.Add(flightRecord);

                isConjunction = true;

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
                var segment = request.Segments.Where(segId => segId.SegmentId.ToUpper().Replace("SEG", "").Equals(i.ToString())).FirstOrDefault();

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
                    FlightDate = DateTime.Parse(segment.ScheduledDepartureTime.LocalDateTime).ToString("yyyyMMdd"),
                    FlightTime = DateTime.Parse(segment.ScheduledDepartureTime.LocalDateTime).ToString("HH:mm:ss"),
                    FlightNumber = segment.MarketingFlightNumber ?? segment.OperatingFlightNumber
                });

                // Identify Arank Segments
                if (i + 1 <= request.Segments.Count())
                {
                    var next = segmentList[i];

                    if (!string.Equals(segment.DestinationAirportCode, next.OriginAirportCode, StringComparison.OrdinalIgnoreCase))
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
                            var baggage = request.DataList.BaggageAllowances.FirstOrDefault(b => b.BaggageAllowanceId == bagRefId && b.BaggageAllowanceTypeCode.Equals("Checked", StringComparison.OrdinalIgnoreCase));

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
    }

}
