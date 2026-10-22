using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Mapper
{
    public class OrderRefundMapper
    {
        public static PaymentAuthorizeRequest OrderRefundRequest(AuthorizePaymentRequest request)
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
                },
                FormOfPayment = MapFormOfPayments(request),
                Orders = MapOrders(request),
                FullRefund = request.IsRfc ? true : false,
                TrackID = request.TrackId
            };
        }

        private static IEnumerable<FormOfPayment> MapFormOfPayments(AuthorizePaymentRequest request)
        {
            var paymentInfo = request.PaymentDetails.First();

            var formOfPayment = paymentInfo.FormOfPayments;

            return
            [
                new()
                {
                    Amount = int.Parse(paymentInfo.Amount.MonetaryAmount),
                    Currency = paymentInfo.Amount.BaseCurrency,
                    Type = formOfPayment == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? $"{Constants.Application.CardType}{request.PaymentDetails.First().CardMethod.CardType}" : formOfPayment == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay? Constants.Application.EasyPayFopType: Constants.Application.FopType,
                    Payer = formOfPayment == Domain.Payment.FormOfPayment.CardEnum ? new()
                    {
                        Name = $"{paymentInfo?.Payer?.LastName} {paymentInfo?.Payer?.FirstName}",
                        PhoneNumber =$"{paymentInfo?.Payer?.PayerContacts?.CountryDailingCode}-{paymentInfo?.Payer?.PayerContacts?.Phone}",
                        Email = paymentInfo?.Payer?.PayerContacts?.Email,
                    } : null,
                }
            ];
        }

        private static IEnumerable<Models.Order> MapOrders(AuthorizePaymentRequest request)
        {
            var orders = request.CustomerDetails.Select(cust => new Models.Order
            {
                TransactionCode = Constants.Application.IfgRefundTransactionCode,
                ReportedFormOfPayment = MapReportedFormOfPayments(request, cust),
                FlightRecords = MapFlightRecords(cust, request),
            });

            return orders;
        }

        private static IEnumerable<ReportedFormOfPayment> MapReportedFormOfPayments(AuthorizePaymentRequest request, Customer cust)
        {
            var orderItems = request.OrderHistory != null && request.OrderHistory.Any() ? request.OrderHistory.First().OrderItems : request.OrderItems;
            var matchingOrderItems = orderItems.Where(x => x.Services.Any(service =>
                service.ServiceBeneficiaries.Contains(cust.CustId) &&
                service.Product.ProductType != Product.ProductTypes.SeatEnum));
            var refundableOrderItems = request.OrderHistory != null && request.OrderHistory.Any() && matchingOrderItems.Any(x => x.RefundPrice == null)
                ? matchingOrderItems.Where(x => x.RefundPrice == null && x.PriceDetails != null && x.PriceDetails.Any())
                : matchingOrderItems.Where(x => x.RefundPrice != null && x.RefundPrice.Any());

            var refundOrderItem = refundableOrderItems.FirstOrDefault();

            var basePriceAmount = refundOrderItem == null
                ? null
                : refundOrderItem.RefundPrice != null && refundOrderItem.RefundPrice.Any()
                    ? refundOrderItem.RefundPrice.First().Amount
                    : refundOrderItem.PriceDetails.First().Amount;

            return new List<ReportedFormOfPayment>
            {
                new()
                {
                    Amount   = int.Parse(decimal.TryParse(basePriceAmount?.MonetaryAmount, out var parsedAmount) ? ((int)(Math.Abs(parsedAmount) * 100)).ToString(): null),
                    Currency = basePriceAmount.BaseCurrency+ "2",
                    Type = request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum ? $"{Constants.Application.CardType}{request.PaymentDetails.First().CardMethod.CardType}" : request.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay ? Constants.Application.EasyPayFopType: Constants.Application.FopType,
                }
            };
        }

        private static IEnumerable<FlightRecord> MapFlightRecords(Customer customerDetails, AuthorizePaymentRequest request)
        {
            var coupons = request.OrderItems.Where(x => x.AccountableDocumentRefs != null && x.Services.All(svc => svc.Product.ProductType == Product.ProductTypes.FlightEnum)).SelectMany(oi => oi.AccountableDocumentRefs.Where(a => a.CustomerRefId == customerDetails.CustId).SelectMany(a => a.Coupons.Select(coupon => new {

                Coupon = coupon,

                DocumentRef = a
            }))).ToList();
            
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
                    DocumentDetails = new DocumentDetails()
                    {
                        RefundDetails = new RefundDetails()
                        {
                             DataInputStatusIndicator = Constants.Application.DataInputStatusIndicator,
                             RefundDocumentNumber = request.ExchangeRefund ? ticketCoupon.DocumentRef.OriginalDocumentNumber : ticketCoupon.DocumentRef.DocumentNumber,
                             RelatedTicketDocumentNumber = request.ExchangeRefund ? ticketCoupon.DocumentRef.OriginalDocumentNumber : ticketCoupon.DocumentRef.DocumentNumber,
                             RelatedTicketDocumentCouponNumberIdentifier = GenerateCouponIdentifier(coupons.Count)
                        }
                    },
                    DateOfIssuance = DateTime.ParseExact(request.OrderItems.First().AccountableDocumentRefs.First().IssueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                    AutomaticRepricingEngine = " ",
                    StatisticalCode = Constants.Application.StatisticalCodes
                });
            }
            return flightRecords;
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
    }
}
