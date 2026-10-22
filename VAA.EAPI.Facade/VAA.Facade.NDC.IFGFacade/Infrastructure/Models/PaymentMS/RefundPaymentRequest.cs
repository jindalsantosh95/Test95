using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.Facade.NDC.IFGFacade.Constants;
using VAA.Facade.NDC.IFGFacade.Domain;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS
{
    public class RefundPaymentRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<RefundPayment> PaymentDetails { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }

        public static explicit operator RefundPaymentRequest(RefundRequest request)
        {
            var paymentRequest = request.FormOfPayment.First();

            return new RefundPaymentRequest()
            {
                BookingReferences = new()
                {
                    RLOC = request.Orders.First().FlightRecords.First().PassengerRecord.Pnr
                },
                OrderId = $"VS932{request.Orders.First().FlightRecords.First().PassengerRecord.Pnr}{DateTime.UtcNow:yy}",
                PaymentDetails = [
                    new()
                    {
                        PaymentTransactionId = request.TransactionId,
                        RefundAmount = new()
                        {
                            MonetaryAmount = (paymentRequest.Amount / (decimal)Math.Pow(10, int.Parse(paymentRequest.Currency[^1..]))).ToString(),
                            BaseCurrency = paymentRequest.Currency[..^1]
                        },
                    }
                ]
            };
        }
    }
}
