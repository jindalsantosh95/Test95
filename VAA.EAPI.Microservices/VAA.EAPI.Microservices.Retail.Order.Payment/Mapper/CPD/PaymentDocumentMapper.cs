using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;


namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD
{
    public class PaymentDocumentMapper
    {
        public static void PaymentDocumentUpdateMapper(Order.Payment.Domain.CPD.CapturePaymentRequest request, PaymentTransactionDocument document, bool transactionStatus, GetTransactionStatusResponse transactionResponse)
        {

            document.PaymentDetails = document.PaymentDetails.Select(a =>
            {
                a.Acquirer = transactionResponse.Transaction.PspName;
                a.PaymentApprovalCode = transactionResponse.Transaction.ApprovalCode;

                return a;
            });

            document.Status = transactionStatus ? "Payment Captured" : "Capture Declined";
            document.AccountableDocumentRefs = request.AccountableDocumentRefs;
            document.CustomerDetails = request.CustomerDetails.Select(c => { c.DateOfBirth = null; return c; });
            document.OrderItems = request.OrderItems;
            document.ModifiedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat);
            document.ModifiedBy = Constants.Application.CreatedBy;
            document.BookingReferences = request.BookingReferences;

        }

        public static PaymentTransactionDocument RefundPaymentDocMapper(RefundTransactionDocument refundDoc)
        {
            return new PaymentTransactionDocument()
            {
                DocType = refundDoc.DocType,
                CreatedBy = refundDoc.CreatedBy,
                CreatedOn = refundDoc.CreatedOn,
                Pos = refundDoc.Pos,
                BookingReferences = refundDoc.BookingReferences,
                OrderId = refundDoc.OrderId,
                PaymentDetails = new List<Domain.CPD.Payment>()
                {
                    new Domain.CPD.Payment()
                    {
                        Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                        {
                            BaseCurrency = refundDoc.PaymentDetails.First().RefundAmount?.BaseCurrency,
                            MonetaryAmount = refundDoc.PaymentDetails.First().RefundAmount?.MonetaryAmount,
                            CurrencyId = refundDoc.PaymentDetails.First().RefundAmount?.CurrencyId
                        },
                        PaymentTransactionId = refundDoc.PaymentDetails.First().PaymentTransactionId
                    }
                },
                Status = refundDoc.Status,
                Id = refundDoc.Id,
                PartitionKey = refundDoc.PartitionKey
            };
        }

        public static PaymentTransactionDocument CreateChildPaymentDocument(PaymentTransactionDocument parentDoc, SplitPnrPaymentRequest request)
        {
            var childDoc = Json.Deserialize<PaymentTransactionDocument>(Json.Serialize(parentDoc));

            childDoc.Id = $"{parentDoc.Id}-{request.ChildBookingReference.RLOC}-Split";
            childDoc.OrderId = $"VS932{request.ChildBookingReference.RLOC}{DateTime.UtcNow:yy}";
            childDoc.BookingReferences = request.ChildBookingReference;
            childDoc.PaymentDetails.First().ParentPaymentId = parentDoc.Id;
            childDoc.CustomerDetails = parentDoc?.CustomerDetails?.Where(cd => request.SplitPaxIds.Contains(cd.CustId));

            if(parentDoc.OrderItems != null)
            {

                if (parentDoc.OrderItems.Any(x => x.ProductType == Domain.Enums.ProductType.Seat))
                {
                    var accountableDocIds = parentDoc.OrderItems.Where(oi => oi.CustRefIds.Any(id => request.SplitPaxIds.Contains(id))).SelectMany(oi => oi.AccountableDocumentRefIds) .Distinct();
                    childDoc.AccountableDocumentRefs = parentDoc.AccountableDocumentRefs?.Where(ad => accountableDocIds.Contains(ad.DocumentNumber));
                }
                else
                {
                    childDoc.AccountableDocumentRefs = parentDoc.AccountableDocumentRefs?.Where(ad => childDoc.CustomerDetails.Any(cd => ad.CustomerRefId.Contains(cd.CustId)));
                }

                var matches = request.SplitPaxIds.SelectMany(splitpax => parentDoc?.OrderItems.Where(orderItem => orderItem.CustRefIds.Contains(splitpax)).Select(orderItem => (orderItem, splitpax)));

                foreach (var (orderItem, splitpax) in matches)
                {
                    orderItem.CustRefIds = orderItem.CustRefIds.Where(id => id == splitpax);
                }

                childDoc.OrderItems = parentDoc.OrderItems;

                int index = 1;
                foreach (var orderItem in childDoc.OrderItems)
                {
                    orderItem.OrderItemId = $"{childDoc.OrderId}-{index++}";
                }

            }

            return childDoc;
        }

        public static CreatePaymentDocumentRequest Mapper(PaymentDocRequest paymentDocRequest, string transactionId)
        {
            var payment = paymentDocRequest.PaymentDetails.First();

            var paymentDetails = new Domain.CPD.Payment()
            {
                FormOfPayments = payment.FormOfPayments,
                TrackId = payment.TrackId,
                Amount = payment.Amount,
                PaymentApprovalCode = payment.ApprovalCode,
                CardMethod = payment.CardMethod
            };

            return new CreatePaymentDocumentRequest()
            {
                PaymentDocumentId = transactionId,
                DocType = paymentDocRequest.Type,
                CreatedBy = Constants.Application.CreatedBy,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                Pos = paymentDocRequest.Pos,
                Parties = paymentDocRequest.Parties,
                BookingReferences = paymentDocRequest.BookingReferences,
                OrderId = paymentDocRequest.OrderId,
                PaymentDetails = [paymentDetails],
                ProductType = paymentDocRequest.ProductType,
            };
        }
    }
}
