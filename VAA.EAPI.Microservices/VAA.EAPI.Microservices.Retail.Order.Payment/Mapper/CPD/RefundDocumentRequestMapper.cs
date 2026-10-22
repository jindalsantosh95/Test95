using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD
{
    public class RefundDocumentRequestMapper
    {
        public static CreateRefundDocumentRequest CreateRefundDocumentMapper(RefundPaymentRequest request, string paymentTransactionId, PaymentResponse refundResponse, PaymentTransactionDocument transactionDocument)
        {
            var paymentDoc = new CreateRefundDocumentRequest
            {
                DocType = "PaymentRefundDoc",
                TransactionId = paymentTransactionId,
                CreatedBy = Constants.Application.CreatedBy,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                ModifiedBy = string.Empty,
                ModifiedOn = string.Empty,
                BookingReferences = request.BookingReferences,
                Parties = request.Parties,
                Pos = request.Pos,
                OrderId = request.OrderId,

                PaymentDetails = new List<RefundPayment>()
                {
                    new RefundPayment()
                    {
                        FormOfPayments=EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum,
                        PaymentTransactionType="Refund",
                        PaymentTransactionId =request.PaymentDetails.FirstOrDefault().PaymentTransactionId,
                        RefundAmount= new RefundAmount()
                        {
                            MonetaryAmount=request.PaymentDetails.FirstOrDefault().RefundAmount.MonetaryAmount,
                            BaseCurrency=request.PaymentDetails.FirstOrDefault().RefundAmount.BaseCurrency
                        },
                        CardType=  request.PaymentDetails.FirstOrDefault().CardType

                    },

                },

                AccountableDocumentRefs = request.AccountableDocumentRefs,
                Status = Constants.Application.RefundStatus,

            };

            return paymentDoc;
        }
    }
}
