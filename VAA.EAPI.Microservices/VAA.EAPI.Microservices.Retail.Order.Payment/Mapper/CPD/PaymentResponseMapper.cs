using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;
using Status = VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD.Status;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD
{
    public class PaymentResponseMapper
    {
        public static Domain.CPD.PaymentResponse CPDPaymentResponseMapper(InitializePaymentResponse initializeResponse, Infrastructure.Models.CPD.PaymentResponse authorizeResponse)
        {
            if (initializeResponse.Transaction?.Id != null)
            {
                if (authorizeResponse.Status.Any(x => x.Code == "2000"))
                    return new Domain.CPD.PaymentResponse
                    {
                        TransactionId = initializeResponse.Transaction.Id,
                        OrderID = initializeResponse.Transaction.OrderNo,
                        Amount = initializeResponse.Transaction.Amount.Value / 100.00,
                        TransactionStatus = authorizeResponse.Status.First().Text
                    };

                else
                {

                    var errors = authorizeResponse.Status.Select(err =>
                    ErrorMapper.MapCPDExceptionToEAPIException(ErrorCodes.CpdErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath,
                    !(string.IsNullOrEmpty(err.SubCode)) ? $"CPDCode: {err.Code}; CPDSubCode: {err.SubCode}; CPDMessage: {err.Text}" : $"CPDCode: {err.Code}; CPDMessage: {err.Text}"));

                    return new Domain.CPD.PaymentResponse
                    {
                        Errors = errors
                    };
                }
            }

            else
            {

                var errors = initializeResponse.Status.Select(err =>
                ErrorMapper.MapCPDExceptionToEAPIException(ErrorCodes.CpdErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath,
                !(string.IsNullOrEmpty(err.SubCode)) ? $"CPDCode: {err.Code}; CPDSubCode: {err.SubCode}; CPDMessage: {err.Text}" : $"CPDCode: {err.Code}; Message: {err.Text}"));

                return new Domain.CPD.PaymentResponse
                {
                    Errors = errors
                };
            }

        }

        public static Domain.CPD.PaymentResponse CapturePaymentResponseMapper(Infrastructure.Models.CPD.CapturePaymentRequest capturePaymentRequest, Infrastructure.Models.CPD.PaymentResponse captureResponse, GetTransactionStatusResponse transactionStatusResponse, PaymentTransactionDocument paymentDoc)
        {
            if (captureResponse.Status.Any(x => x.Code == "100"))
                return new Domain.CPD.PaymentResponse
                {
                    TransactionId = capturePaymentRequest.UpdateOrderData.Transaction.Id,
                    OrderID = capturePaymentRequest.UpdateOrderData.Transaction.OrderNo,
                    Amount = capturePaymentRequest.UpdateOrderData.Transaction.Orders.LineItem.First().Amount.Value / 100.00,
                    TransactionStatus = captureResponse.Status.First().Text,
                    MaskedCardNumber = paymentDoc.PaymentDetails.First().CardMethod.CardNumber,
                    ExpiryDate = transactionStatusResponse.Transaction.CardExpiry,
                    CardType = paymentDoc.PaymentDetails.First().CardMethod.CardType.ToString(),
                    PaymentCommitedDateTime = paymentDoc.ModifiedOn,
                    ApprovalCode = transactionStatusResponse.Transaction.ApprovalCode
                };

            else
            {
                var errors = captureResponse.Status.Select(err =>
                   ErrorMapper.MapCPDExceptionToEAPIException(ErrorCodes.CpdErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath,
                   !(string.IsNullOrEmpty(err.SubCode)) ? $"CPDCode: {err.Code}; CPDSubCode: {err.SubCode}; CPDMessage: {err.Text}" : $"CPDCode: {err.Code}; CPDMessage: {err.Text}"));

                return new Domain.CPD.PaymentResponse
                {
                    Errors = errors
                };
            }
        }

        public static Domain.CPD.PaymentResponse RefundPaymentResponseMapper(Infrastructure.Models.CPD.RefundPaymentRequest refundPaymentRequest, Infrastructure.Models.CPD.PaymentResponse refundResponse, PaymentTransactionDocument capturedPaymentDocument)
        {

            if (refundResponse.Status.Any(x => x.Code == "1000" || x.Code == "1001") && refundResponse.Status.First().Exception is null)
                return new Domain.CPD.PaymentResponse
                {
                    TransactionId = refundPaymentRequest.Refund.Transaction.Id,
                    OrderID = refundPaymentRequest.Refund.Transaction.OrderNo,
                    Amount = refundPaymentRequest.Refund.Transaction.Amount.Value / 100.00,
                    TransactionStatus = refundResponse.Status.First().Text,
                    PaymentCommitedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                    MaskedCardNumber = capturedPaymentDocument.PaymentDetails.First().CardMethod.CardNumber,
                    CardType = capturedPaymentDocument.PaymentDetails.First().CardMethod.CardType.ToString()
                };

            else
            {
                return new Domain.CPD.PaymentResponse
                {
                    Errors = refundResponse.Status.Select(err => ErrorMapper.MapCPDExceptionToEAPIException(ErrorCodes.CpdErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, BuildErrorMessage(err)))
                };
            }
        }

        private static string BuildErrorMessage(Status err)
        {
            string baseMessage = $"Code: {err.Exception?.Code.Text ?? err.Code};";
            if (!string.IsNullOrEmpty(err.SubCode))
            {
                baseMessage += $" SubCode: {err.SubCode};";
            }
            return baseMessage + $" Message: {err.Text ?? "Refund Failed"}";
        }

        public static IEnumerable<Domain.CPD.PaymentResponse> SplitPnrPaymentResponse(PaymentTransactionDocument childDoc)
        {
            return childDoc?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum || x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false ? new List<Domain.CPD.PaymentResponse>
             {
                new Domain.CPD.PaymentResponse
                {
                    TransactionId = childDoc.Id
                }
                }
               : new List<Domain.CPD.PaymentResponse>
               {
                    new Domain.CPD.PaymentResponse
                    {
                        TransactionId = childDoc.Id,
                        OrderID = childDoc.OrderId,
                        Amount = Convert.ToDouble(childDoc.PaymentDetails.First().Amount.MonetaryAmount),
                        TransactionStatus = childDoc.PaymentDetails.First().PaymentProcessingStatus.ToString(),
                        MaskedCardNumber = childDoc.PaymentDetails.First().CardMethod.CardNumber,
                        ExpiryDate = childDoc.PaymentDetails.First().CardMethod.ExpiryDate,
                        CardType = childDoc.PaymentDetails.First().CardMethod.CardType.ToString(),
                        PaymentCommitedDateTime = childDoc.CreatedOn?.ToString()
                    }
               };
        }
       
        public static Domain.CPD.PaymentResponse ResponseMapper(CreatePaymentDocumentRequest paymentDocumentRequest)
        {
            return new Domain.CPD.PaymentResponse
            {
                TransactionId = paymentDocumentRequest.PaymentDocumentId,
                OrderID = paymentDocumentRequest.OrderId,
                PaymentCommitedDateTime = paymentDocumentRequest.CreatedOn,
                Amount = Convert.ToDouble(paymentDocumentRequest.PaymentDetails.First().Amount.MonetaryAmount),
                FormOfPayment = paymentDocumentRequest.PaymentDetails.First().FormOfPayments.Value.ToString()
            };
        }
    }
}
