using System.Globalization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Mapper
{
    public class TransactionMapper
    {
        public static TransactionServiceRequest MapArcTranctionRequest(Domain.TransactionRequest request)
        {
            return new TransactionServiceRequest()
            {
                TSCD = ConfigReader.Get(Constants.Environment.TicketingSystemCode),
                TPST = ConfigReader.Get(Constants.Environment.ARCEnvironmentName),
                Transactions = request.ARCRequestType == Constants.Enums.ARCRequestType.Rfc ? MapCannTransactions(request) : request.ARCRequestType == Constants.Enums.ARCRequestType.NonRfc ? MapRefundTransactions(request) : MapTransactions(request)
            };
        }

        private static IEnumerable<Transaction> MapRefundTransactions(TransactionRequest request)
        {
            var agentId = request.Parties.Participants.First(p => p.PartyType.Value == EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller && p.Ordinal.StartsWith("1")).AgencyID;

            var currencyCode = request.PaymentDetails.First().Amount.BaseCurrency;

            var transactions = new List<Transaction>();
            IEnumerable<VAA.EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef> accountableDocument;

            foreach (var customer in request.DataList.CustomerDetails)
            {

                if (request.AccountableDocumentRef != null)
                {
                    accountableDocument = request.AccountableDocumentRef.Where(a => a.CustomerRefId == customer.CustId);
                }
                else
                {
                    accountableDocument = request.OrderItems.SelectMany(item => item.AccountableDocumentRefs).Where(a => a.CustomerRefId == customer.CustId);
                }

                var accDocs = accountableDocument.FirstOrDefault();
                var orderItems = request.OrderItems.Where(x => x.Services.FirstOrDefault()?.ServiceBeneficiaries.Contains(customer.CustId) == true);

                var paymentDetail = request.PaymentDetails.First();

                var amount = request.OrderItems?.FirstOrDefault(oi => oi.Services?.Any(s => s.ServiceBeneficiaries?.Contains(customer.CustId) == true) == true)?.RefundPrice.First().Amount.MonetaryAmount;
                var ttds = (int)(decimal.Parse(amount.Replace("-", "")) * 100);

                int segmentCount = request.DataList.SegmentDetails.Take(4).Count();

                transactions.Add(new Transaction()
                {
                    ValidatingCarrier = request.TicketStockNumber,
                    CurrencyType = currencyCode,
                    Record2 = new Record2()
                    {
                        CUIN = "RRRR",
                        ANCD = agentId,
                        TDOI = DateTime.ParseExact(accDocs.IssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyMMdd"),
                        SCDE = Constants.Application.InternationalStatisticalCode,
                        TDNM = accDocs.DocumentNumber.Substring(3),
                        DTYP_TCDF = Constants.Application.Refund,
                        SASP = Constants.Application.AirlineCode,
                        TDTM = DateTime.UtcNow.ToString("ddMMMyy/HHmmss'Z'", CultureInfo.InvariantCulture),
                        PNME = $"{customer.LastName}/{customer.FirstName}",
                        TICC = "US"
                    },
                    Record3 = new()
                    {
                        TDOI = DateTime.ParseExact(accDocs.IssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyMMdd"),
                        RelatedDocOne = new()
                        {
                            CUIN = (string.Concat(Enumerable.Range(1, segmentCount)).PadRight(4, '0')),
                            VCRR = request.TicketStockNumber,
                            TDNM = accDocs.DocumentNumber.Substring(3)
                        }
                    },
                    Record5 = MapRecord5(customer, request, orderItems, accDocs),
                    Record8 = [new() {
                        FORM_OF_PAYMENT_ONE = new()
                        {
                            FOPA = request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? request.PaymentDetails.First().CardMethod.CardNumber : null,
                            FOPB = ttds,
                            FOPT = paymentDetail?.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? $"TC{paymentDetail.CardMethod.CardType}{paymentDetail.CardMethod.CardNumber.Substring(0, 4)}" : "RF"
                        }
                    }]
                });

            }
            return transactions;
        }

        private static IEnumerable<Transaction> MapCannTransactions(TransactionRequest request)
        {
            var agentId = request.Parties.Participants.First(p => p.PartyType.Value == EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller && p.Ordinal.StartsWith("1")).AgencyID;

            var currencyCode = request.PaymentDetails.First().Amount.BaseCurrency;

            var accountableDocument = new VAA.EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef();
            var transactions = new List<Transaction>();

            foreach (var customer in request.DataList.CustomerDetails)
            {
                accountableDocument = request.OrderItems.SelectMany(item => item.AccountableDocumentRefs).FirstOrDefault(a => a.CustomerRefId == customer.CustId);

                transactions.Add(new Transaction()
                {
                    ValidatingCarrier = request.TicketStockNumber,
                    CurrencyType = currencyCode,
                    Record2 = new Record2()
                    {
                        ANCD = agentId,
                        TDOI = DateTime.ParseExact(accountableDocument.IssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyMMdd"),
                        TDNM = accountableDocument.DocumentNumber.Substring(3),
                        DTYP_TCDF = Constants.Application.Cann,
                        SASP = Constants.Application.AirlineCode,
                        TDTM = DateTime.UtcNow.ToString("ddMMMyy/HHmmss'Z'", CultureInfo.InvariantCulture),
                        TICC = "US"
                    }
                });

            }

            return transactions;
        }

        private static IEnumerable<Transaction> MapTransactions(Domain.TransactionRequest request)
        {
            var agentId = request.Parties.Participants.First(p => p.PartyType.Value == EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller && p.Ordinal.StartsWith("1")).AgencyID;

            var currencyCode = request.PaymentDetails.First().Amount.BaseCurrency;

            var transactions = new List<Transaction>();
            var segments = request.DataList.SegmentDetails;
            var customerDetails = request.ARCRequestType == Constants.Enums.ARCRequestType.NameCorrection ? request.DataList.CustomerDetails.Where(x => x.IsNameChanged) : request.DataList.CustomerDetails;

            IEnumerable<VAA.EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef> accountableDocument;

            foreach (var customer in customerDetails)
            {
                if (request.AccountableDocumentRef != null)
                {
                    accountableDocument = request.AccountableDocumentRef.Where(a => a.CustomerRefId == customer.CustId);
                }
                else
                {
                    accountableDocument = request.OrderItems.SelectMany(item => item.AccountableDocumentRefs).Where(a => a.CustomerRefId == customer.CustId);
                }

                var accountableDocumentRefs = accountableDocument.ToList();
                var accDoc = accountableDocumentRefs.FirstOrDefault();
                var conjDoc = accountableDocumentRefs.ElementAtOrDefault(1) ?? accDoc;
                var orderItems = request.OrderItems.Where(x => x.Services.FirstOrDefault()?.ServiceBeneficiaries.Contains(customer.CustId) == true);

                var (cuin, splitIndex) = MapCouponUseIndicator(segments);

                var segmentDetails = segments.Take(splitIndex);
                var conjSegments = segments.Skip(splitIndex);

                var coupons = MapCoupons(accDoc, segmentDetails, request);
                var conjCoupons = MapCoupons(conjDoc, conjSegments, request, 1);

                var transaction = new Transaction()
                {
                    ValidatingCarrier = request.TicketStockNumber,
                    CurrencyType = currencyCode,
                    Record2 = MapRecord2(agentId, customer, request, cuin.Substring(0, Math.Min(4, cuin.Length)), accDoc),
                    Record4 = MapRecord4(agentId, request, accDoc, customer, orderItems, currencyCode),
                    Record5 = MapRecord5(customer, request, orderItems, accDoc),
                    Record6 = MapRecord6(coupons),
                    Record7 = [MapRecord7(customer, request, orderItems)],
                    Record8 = (request.ARCRequestType == Constants.Enums.ARCRequestType.AddCollect || request.ARCRequestType == Constants.Enums.ARCRequestType.NameCorrection || request.ARCRequestType == Constants.Enums.ARCRequestType.EvenExchange) ? MapRecord8forExchange(customer, request.PaymentDetails, request, accDoc) : new[] { MapRecord8(customer, request.PaymentDetails, request, accDoc) },
                    Record9 = new() { ERST = Constants.Application.FareEndorsement, FOPI = request.PaymentDetails.First().FormOfPayments == PaymentDetails.FormOfPayment.CashEnum ? Constants.Application.Cash : Constants.Application.Card },
                    RecordA = request.ProductType == Constants.Enums.ProductType.Seat ? MapRecordA(accDoc, request) : null,
                    RecordG = request.ProductType == Constants.Enums.ProductType.Seat ? MapRecordG(request.OrderItems, customer, request) : null

                };

                if (conjSegments.Any())
                {
                    transaction.Conjunctions = new List<Conjunctions>
                    {
                       new Conjunctions
                       {
                            Record2 = MapRecord2(agentId, customer, request, cuin.Substring(4, 4), conjDoc),
                            Record6 = MapRecord6(conjCoupons)
                       }
                    };
                }


                transactions.Add(transaction);
            }

            return transactions;
        }

        private static IEnumerable<RecordG> MapRecordG(IEnumerable<OrderItems> orderItems, Customer customer, TransactionRequest request)
        {
            var recordGs = new List<RecordG>();

            var segmentId = request.DataList.SegmentDetails.First().SegmentId;

            var icwd = request.OrderItems.Where(o => o.Services.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum)
                    .Where(o => o.Services.FirstOrDefault()?.SegmentRefIds?.FirstOrDefault() == segmentId && o.Services.FirstOrDefault()?.ServiceBeneficiaries?.Contains(customer.CustId) == true)
                    .Select(o => o.AccountableDocumentRefs.FirstOrDefault(a => a.CustomerRefId == customer.CustId)?.DocumentNumber).First();

            var seatOrderItems = request.SeatDetails.Where(s => s.CustomerRefId == customer.CustId);

            foreach (var orderItem in orderItems.Where(a => a.Services.First().Product != null && a.Services.First().Product.ProductType == Product.ProductTypes.SeatEnum && a.Services.First().ServiceBeneficiaries.First() == customer.CustId && a.Services.First().SegmentRefIds.First() == segmentId))
            {
                int i = 1;

                var seatAmount = seatOrderItems.First(a => a.ElectronicMiscellaneousDocumentNumber == orderItem.AccountableDocumentRefs.First().DocumentNumber).Amount.MonetaryAmount;

                recordGs.Add(new RecordG
                {
                    CNMB = i,
                    FOAD = Constants.Application.VS,
                    RFIS = Constants.Application.RFIS,
                    CVLU = int.Parse(seatAmount),
                    ICWC = i,
                    ICWD = icwd
                });

            }

            return recordGs;
        }

        private static RecordA MapRecordA(EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef accountableDocument, TransactionRequest request)
        {
            var icwd = string.Empty;

            var custId = accountableDocument.CustomerRefId;
            var segmentId = request.DataList.SegmentDetails.First().SegmentId;

            icwd = request.OrderItems.Where(o => o.Services.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum)
                        .Where(o => o.Services.FirstOrDefault()?.SegmentRefIds?.FirstOrDefault() == segmentId && o.Services.FirstOrDefault()?.ServiceBeneficiaries?.Contains(custId) == true)
                        .Select(o => o.AccountableDocumentRefs.FirstOrDefault(a => a.CustomerRefId == custId)?.DocumentNumber).First();

            return new RecordA()
            {
                ICWD = icwd,
                RFIC = Constants.Application.RFIC
            };
        }

        private static IEnumerable<Record6> MapRecord6(IEnumerable<Models.Coupon> coupons)
        {
            var source = coupons.ToArray();
            var groups = new List<Record6>();

            int index = 0;

            if (source.Length > 0)
            {
                groups.Add(new Record6
                {
                    FIRST_COUPON = source.ElementAtOrDefault(index),
                    SECOND_COUPON = source.ElementAtOrDefault(index + 1),
                    THIRD_COUPON = source.ElementAtOrDefault(index + 2)
                });

                index += 3;
            }

            if (index < source.Length)
            {
                groups.Add(new Record6
                {
                    FIRST_COUPON = source[index]
                });

                index += 1;
            }

            return groups;
        }

        private static IEnumerable<Models.Coupon> MapCoupons(EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef accountableDocument, IEnumerable<Segment> segmentDetails, TransactionRequest request, int startCounter = 1)
        {
            var couponList = new List<Models.Coupon>();
            int couponCounter = startCounter;

            Segment previousSegment = null;

            foreach (var segment in segmentDetails)
            {
                if (previousSegment != null)
                {
                    if (segment.OriginAirportCode != previousSegment.DestinationAirportCode) { couponCounter += 2; }
                    else { couponCounter++; }
                }

                if (couponCounter > 4)
                {
                    couponCounter = ((couponCounter - 1) % 4) + 1;
                }

                var couponDocument = accountableDocument.Coupons.First(a => a.SegmentRefId == segment.SegmentId);

                var coupon = new Models.Coupon()
                {
                    SOAC = segment.OriginAirportCode,
                    SDAC = segment.DestinationAirportCode,
                    SADS = "VS",
                    SFDD_DOSR = DateTime.Parse(segment.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM", CultureInfo.InvariantCulture).ToUpper(),
                    CNMB = couponCounter
                };

                if (request.ProductType == Constants.Enums.ProductType.Ticket)
                {
                    coupon.SRBD = segment.ClassOfService;
                    coupon.NVBD = !string.IsNullOrEmpty(couponDocument.TicketEffectiveDate) ? couponDocument.TicketEffectiveDate[..^2] : null;
                    coupon.NVAD = !string.IsNullOrEmpty(couponDocument.TicketExpirationDate) ? couponDocument.TicketExpirationDate[..^2] : null;
                    coupon.FBSS = couponDocument?.FareBasis ?? segment?.FareBasis;
                    coupon.SFNM = segment.MarketingFlightNumber;
                    coupon.SFDT = DateTime.Parse(segment.ScheduledDepartureTime.LocalDateTime).ToString("HHmm");
                    coupon.SCDF = segment?.ConnectionOrStopOver;
                    coupon.SRSC = "OK";
                }

                couponList.Add(coupon);
                previousSegment = segment;
            }

            return couponList;
        }

        private static Record8 MapRecord8(Customer customer, IEnumerable<PaymentDetails> paymentDetails, TransactionRequest request, EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef accountableDocument)
        {
            var amount = decimal.TryParse(request.OrderItems?.FirstOrDefault(oi => oi.Services?.Any(s => s.ServiceBeneficiaries?.Contains(customer.CustId) == true) == true)?.PriceDetails?.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount, out var parsed) ? ((int)(parsed * 100)).ToString() : null;

            if (request.ProductType == Constants.Enums.ProductType.Seat)
            {
                amount = request.SeatDetails.First(a => a.ElectronicMiscellaneousDocumentNumber == accountableDocument.DocumentNumber).Amount.MonetaryAmount;
            }

            var paymentDetail = paymentDetails.First();

            var record8 = new Record8()
            {
                FORM_OF_PAYMENT_ONE = new()
                {
                    FOPA = paymentDetail?.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? paymentDetail.CardMethod.CardNumber : null,
                    FOPB = int.Parse(amount),
                    ACDE = paymentDetail?.ApprovalCode,
                    FOPT = paymentDetail?.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? $"CC{paymentDetail.CardMethod.CardType}{paymentDetail.CardMethod.CardNumber.Substring(0, 4)}" : Constants.Application.Cash,
                    EDCC = paymentDetail?.CardMethod?.ExpiryDate?.Replace("/", ""),
                    AAMN = paymentDetail?.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum ? int.Parse(amount) : null,
                }
            };

            return record8;
        }

        private static IEnumerable<Record8> MapRecord8forExchange(Customer customer, IEnumerable<PaymentDetails> paymentDetails, TransactionRequest request, EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef accountableDocument)
        {
            var exchnageDeatils = request.ARCDocument.ARCTransactionHistory.Where(x => x.CustomerId == customer.CustId).SelectMany(x => x.PassengerTicketDetails).OrderBy(x => x.Version).ToList();

            var records = new List<Record8>();
            var payment = paymentDetails.FirstOrDefault();

            if (!exchnageDeatils.Any()) return records;

            int lastAddCollectIndex = exchnageDeatils.LastOrDefault().Version;

            Domain.PassengerTicketDetails previousExchange = null;

            for (int i = 0; i < exchnageDeatils.Count; i++)
            {
                var currentExchange = exchnageDeatils[i];

                bool isEvenOrNameCorrection = currentExchange.Type == Constants.Enums.ARCRequestType.EvenExchange.ToString() || currentExchange.Type == Constants.Enums.ARCRequestType.NameCorrection.ToString();

                if (isEvenOrNameCorrection && i == exchnageDeatils.Count - 1) continue;

                if (previousExchange == null)
                {
                    previousExchange = currentExchange;
                    continue;
                }

                AddRecord(records, previousExchange, currentExchange, payment, lastAddCollectIndex);
                previousExchange = null;
            }

            if (previousExchange != null)
            {
                AddRecord(records, previousExchange, null, payment, lastAddCollectIndex);
            }

            return records;
        }

        private static void AddRecord(List<Record8> records, Domain.PassengerTicketDetails first, Domain.PassengerTicketDetails second, PaymentDetails payment, int lastAddCollectIndex)
        {
            bool firstIsPaidAddCollect = first.Type == Constants.Enums.ARCRequestType.AddCollect.ToString() && first.Version == lastAddCollectIndex;
            bool secondIsPaidAddCollect = second != null && second.Type == Constants.Enums.ARCRequestType.AddCollect.ToString() && second.Version == lastAddCollectIndex;

            records.Add(new Record8
            {
                FORM_OF_PAYMENT_ONE = BuildFormOfPayment(firstIsPaidAddCollect, payment, first.Amount?.MonetaryAmount, first.AccountableDocumentRefs.FirstOrDefault()?.DocumentNumber),
                FORM_OF_PAYMENT_TWO = second == null ? null : BuildFormOfPayment(secondIsPaidAddCollect, payment, second.Amount?.MonetaryAmount, second.AccountableDocumentRefs.FirstOrDefault()?.DocumentNumber)
            });
        }
        private static FormOfPayment BuildFormOfPayment(bool isAddcollect, PaymentDetails paymentDetail, string amount, string docNumber)
        {

            return new FormOfPayment()
            {
                FOPA = isAddcollect ? paymentDetail?.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? !string.IsNullOrEmpty(paymentDetail?.CardMethod?.CardNumber) ? paymentDetail.CardMethod.CardNumber : $"932{docNumber.Substring(3)}{long.Parse(docNumber) % 7}ALL" : null : $"932{docNumber.Substring(3)}{long.Parse(docNumber) % 7}ALL",
                FOPB = (int)(decimal.TryParse(amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var val) ? val * 100 : 0),
                FOPT = isAddcollect ? paymentDetail?.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? $"CC{paymentDetail?.CardMethod?.CardType}{paymentDetail?.CardMethod?.CardNumber?.Substring(0, 4)}" : Constants.Application.Cash : Constants.Application.Exchange,
                EDCC = isAddcollect ? paymentDetail?.CardMethod?.ExpiryDate?.Replace("/", "") : null,
                ACDE = isAddcollect ? paymentDetail?.ApprovalCode : null,
            };
        }

        private static Record7 MapRecord7(Customer customer, TransactionRequest request, IEnumerable<OrderItems> orderItems)
        {
            var basePriceComponent = orderItems.FirstOrDefault()?.PriceDetails?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum);

            var record7 = new Record7()
            {
                FCAR = basePriceComponent.FareCalculation.Length > 87 ? basePriceComponent.FareCalculation.Substring(0, 87) : basePriceComponent.FareCalculation,
                FCSN = basePriceComponent.FareCalculation != null ? 1 : 0,
                FCAR2 = basePriceComponent.FareCalculation.Length > 87 ? basePriceComponent.FareCalculation.Substring(87) : null,
                FCSN2 = basePriceComponent.FareCalculation.Length > 87 ? 2 : 0,
                FCPI = Constants.Application.FareCalculationPricingIndicator,
                FCMI = Constants.Application.FareCalculationModeIndicator
            };

            return record7;
        }

        private static IEnumerable<Record5> MapRecord5(Customer customer, TransactionRequest request, IEnumerable<OrderItems> orderItems, EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef accountableDocument)
        {
            if (request.ProductType == Constants.Enums.ProductType.Seat)
            {
                var amount = request.SeatDetails.First(a => a.ElectronicMiscellaneousDocumentNumber == accountableDocument.DocumentNumber).Amount.MonetaryAmount;

                return [new Record5() {
                            TTDS = int.Parse(amount),
                            ARIN = "R"
                }];

            }
            else
            {
                var pricingDetails = (request.ARCRequestType == Constants.Enums.ARCRequestType.NonRfc) ? orderItems.FirstOrDefault()?.SalePriceDifference : orderItems.FirstOrDefault()?.PriceDetails;
                var taxBreakup = pricingDetails?.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.TaxBreakup;
                var basePrice = pricingDetails?.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount;
                var totalAmount = decimal.TryParse(basePrice, out var dec) ? (int)(dec * 100) : 0;

                string penaltyAmount = null;
                int refundPrice = 0;
                if (request.ARCRequestType == Constants.Enums.ARCRequestType.NonRfc)
                {
                    penaltyAmount = (from o in orderItems
                                     from s in o.Services
                                     where s.ServiceBeneficiaries.Contains(customer.CustId)
                                     from penId in s.PenaltyRef?.CancellationPenaltyRefIds ?? Enumerable.Empty<string>()
                                     join p in request.DataList?.Penalty on penId equals p.Id
                                     select p.MaxPrice.Amount.MonetaryAmount).FirstOrDefault();

                    var amount = request.OrderItems?.FirstOrDefault(oi => oi.Services?.Any(s => s.ServiceBeneficiaries?.Contains(customer.CustId) == true) == true)?.RefundPrice.First().Amount.MonetaryAmount;
                    refundPrice = (int)(decimal.Parse(amount.Replace("-", "")) * 100);
                }

                var records = new List<Record5>();

                if (taxBreakup != null && taxBreakup.Any())
                {
                    var mappedTaxes = new List<TaxDetail>();

                    foreach (var tax in taxBreakup.Where(t => t != null))
                    {
                        bool isXF = tax.Code == Constants.Application.TaxCode;
                        mappedTaxes.Add(MapTax(tax, false));

                        if (isXF) { mappedTaxes.Add(MapTax(tax, true)); }
                    }

                    if (request.ARCRequestType == Constants.Enums.ARCRequestType.NonRfc)
                    {
                        mappedTaxes.Add(new TaxDetail
                        {
                            TFCC = "CP",
                            TFCA = (int)Math.Round((decimal.TryParse(penaltyAmount, out var pv) ? pv : 0) * 100m, MidpointRounding.AwayFromZero)
                        });
                    }

                    if (request.ARCRequestType != Constants.Enums.ARCRequestType.NonRfc)
                    {
                        var penaltyAmt = request.DataList?.Penalty?.Any() == true
                            ? (from o in orderItems
                                          from s in o.Services
                                          where s.ServiceBeneficiaries.Contains(customer.CustId)
                                          from penId in s.PenaltyRef?.ChangePenaltyRefIds ?? Enumerable.Empty<string>()
                                          where penId.Contains("-CHANGE", StringComparison.OrdinalIgnoreCase)
                                          join p in request.DataList?.Penalty on penId equals p.Id
                                          select p.MaxPrice.Amount.MonetaryAmount).FirstOrDefault()
                            : null;

                        var changePenalty = string.IsNullOrEmpty(penaltyAmt) ? 0m : decimal.Parse(penaltyAmt, CultureInfo.InvariantCulture);
                        if (request.ARCRequestType == Constants.Enums.ARCRequestType.EvenExchange
                            || (request.ARCRequestType == Constants.Enums.ARCRequestType.AddCollect && !string.IsNullOrEmpty(penaltyAmt)))
                        {
                            var difference = orderItems.FirstOrDefault()?.SalePriceDifference;
                            var baseFare = difference?.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.BaseFareEnum)?.Amount?.MonetaryAmount;
                            var totalTax = difference?.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.Amount?.MonetaryAmount;
                            if (decimal.TryParse(baseFare, NumberStyles.Number, CultureInfo.InvariantCulture, out var fareDifference)
                                && decimal.TryParse(totalTax, NumberStyles.Number, CultureInfo.InvariantCulture, out var taxDifference))
                            {
                                var residualValue = fareDifference + taxDifference;
                                if (residualValue < 0)
                                {
                                    changePenalty -= residualValue;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(penaltyAmt) || changePenalty > 0)
                        {
                            mappedTaxes.Add(new TaxDetail
                            {
                                TFCC = "CP",
                                TFCA = (int)Math.Round(changePenalty * 100m,
                                    MidpointRounding.AwayFromZero)
                            });
                        }
                    }

                    records = mappedTaxes.Chunk(6).Select((chunk, index) =>
                        {
                            var items = chunk.ToArray();

                            return new Record5
                            {
                                TAX_ONE = items.ElementAtOrDefault(0),
                                TAX_TWO = items.ElementAtOrDefault(1),
                                TAX_THREE = items.ElementAtOrDefault(2),
                                TAX_FOUR = items.ElementAtOrDefault(3),
                                TAX_FIVE = items.ElementAtOrDefault(4),
                                TAX_SIX = items.ElementAtOrDefault(5),

                                TTDS = index == 0 ? (request.ARCRequestType == Constants.Enums.ARCRequestType.NonRfc ? refundPrice : totalAmount) : 0,
                                ARIN = "R"
                            };
                        }).ToList();
                }
                return records;
            }
        }

        private static TaxDetail? MapTax(Tax tax, bool isSecondXF)
        {
            if (tax == null) return null;

            decimal amountValue = decimal.TryParse(tax.Amount?.MonetaryAmount, out var val) ? Math.Abs(val) : 0;

            bool isXF = tax.Code == Constants.Application.TaxCode;

            return new TaxDetail
            {
                TFCC = tax.Code,
                TFCA = isXF && isSecondXF ? 0 : (int)Math.Round(amountValue * 100m, MidpointRounding.AwayFromZero),
                TFCS = isXF && isSecondXF ? $"{tax.CollectionPoints?.FirstOrDefault()?.LocationCode}{amountValue.ToString("0.#", CultureInfo.InvariantCulture)}" : null
            };
        }


        private static Record4 MapRecord4(string agentId, TransactionRequest request, EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef accountableDocument, Customer customer, IEnumerable<OrderItems> orderItems, string currencyCode)
        {
            var originDestinations = request.DataList.OriginDestinationAssociations;

            var trueOrginDestination = $"{originDestinations.First().OriginDestination.OriginAirport}{originDestinations.Last().OriginDestination.DestinationAirport}";

            var pricingDetails = orderItems.FirstOrDefault()?.PriceDetails;

            var basePriceComponent = pricingDetails?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum);

            var taxBreakup = pricingDetails?.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.TaxBreakup;

            var totalAmountComponent = pricingDetails?.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.BasePriceEnum);

            string fareAmount = string.Empty;
            string totalAmount = string.Empty;

            if (decimal.TryParse(basePriceComponent?.Amount?.MonetaryAmount, out var parsedBaseFare))
            {
                var currency = basePriceComponent.Amount.BaseCurrency ?? "";
                fareAmount = $"{parsedBaseFare}";
            }

            if (decimal.TryParse(totalAmountComponent?.Amount?.MonetaryAmount, out var parsedTotalAmount))
            {
                var currency = totalAmountComponent.Amount.BaseCurrency ?? "";
                totalAmount = $"{parsedTotalAmount}";
            }

            decimal xtTaxtotal = taxBreakup.Skip(2).Sum(t => decimal.Parse(t.Amount.MonetaryAmount, CultureInfo.InvariantCulture));

            if (request.ProductType == Constants.Enums.ProductType.Seat)
            {
                var amount = request.SeatDetails.First(a => a.ElectronicMiscellaneousDocumentNumber == accountableDocument.DocumentNumber).Amount.MonetaryAmount;

                fareAmount = (int.Parse(amount) / 100m).ToString("0.00");
                totalAmount = fareAmount;
            }

            var record4 = new Record4()
            {
                PRFR = request.BookingReferences.RLOC + "/" + Constants.Application.SabreIndicator,
                OITD = (request.ARCRequestType == Constants.Enums.ARCRequestType.AddCollect || request.ARCRequestType == Constants.Enums.ARCRequestType.NameCorrection || request.ARCRequestType == Constants.Enums.ARCRequestType.EvenExchange) ? accountableDocument.OriginalDocumentNumber : accountableDocument.DocumentNumber,
                OIDT = (request.ARCRequestType == Constants.Enums.ARCRequestType.AddCollect || request.ARCRequestType == Constants.Enums.ARCRequestType.NameCorrection || request.ARCRequestType == Constants.Enums.ARCRequestType.EvenExchange) ? DateTime.ParseExact(accountableDocument.OriginalIssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("ddMMMyy", CultureInfo.InvariantCulture) : DateTime.ParseExact(accountableDocument.IssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("ddMMMyy", CultureInfo.InvariantCulture),
                OIAN = agentId,
                BFAM = fareAmount,
                TTDS = totalAmount,
                BAID = request.BookingReferences.RLOC,
                ANCD = agentId,
                BFAM_CURRENCY = currencyCode,
                SASP = Constants.Application.AirlineCode,
                TODC = request.ProductType == Constants.Enums.ProductType.Seat ? null : trueOrginDestination
            };

            if (request.ProductType != Constants.Enums.ProductType.Seat)
            {
                record4.TAXA = $"{taxBreakup.First().Amount?.MonetaryAmount}{taxBreakup.First().Code}";
                record4.TAXA2 = $"{taxBreakup.ElementAt(1).Amount?.MonetaryAmount}{taxBreakup.ElementAt(1).Code}";
                record4.TAXA3 = $"{xtTaxtotal.ToString("0.00", CultureInfo.InvariantCulture)}XT";
            }

            return record4;
        }

        private static (string cuin, int splitIndex) MapCouponUseIndicator(IEnumerable<Segment> segments)
        {
            var segmentList = segments.ToList();
            int totalCount = segmentList.Count;

            string cuin = "";
            Segment prev = null;
            int logicalCount = 0;
            int splitIndex = totalCount;
            int realIndex = 0;

            foreach (var curr in segmentList)
            {
                if (prev == null)
                {
                    cuin += "F";
                    logicalCount++;
                }
                else
                {
                    bool mismatch = prev.DestinationAirportCode != curr.OriginAirportCode;
                    if (mismatch)
                    {
                        cuin += "S";
                        logicalCount++;

                        if (logicalCount == 4 && splitIndex == totalCount)
                        {
                            splitIndex = realIndex;
                        }
                    }

                    cuin += "F";
                    logicalCount++;

                    if (logicalCount == 4 && splitIndex == totalCount)
                    {
                        splitIndex = realIndex + 1;
                    }
                }

                prev = curr;
                realIndex++;
            }

            int remainder = cuin.Length % 4;
            if (remainder != 0)
            {
                cuin = cuin.PadRight(cuin.Length + (4 - remainder), 'V');
            }

            return (cuin, splitIndex);
        }


        private static Record2 MapRecord2(string agentId, Customer customer, TransactionRequest request, string couponUseIndicator, EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef accountableDocument)
        {
            var record2 = new Record2()
            {
                ANCD = agentId,
                CUIN = couponUseIndicator,
                TDOI = DateTime.ParseExact(accountableDocument.IssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyMMdd"),
                SCDE = Constants.Application.InternationalStatisticalCode,
                TDNM = accountableDocument.DocumentNumber.Substring(3),
                DTYP_TCDF = request.ProductType == Constants.Enums.ProductType.Ticket ? Constants.Application.Tktt : Constants.Application.Emda,
                SASP = Constants.Application.AirlineCode,
                PNME = $"{customer.LastName}/{customer.FirstName}",
                TDTM = DateTime.UtcNow.ToString("ddMMMyy/HHmmss'Z'", CultureInfo.InvariantCulture),
                TICC = "US"
            };

            return record2;
        }
    }
}
