using System;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD
{
    public class PaymentDocumentRequestMapper
    {
        public static CreatePaymentDocumentRequest CreateRequestMapper(AuthorizePaymentRequest authorizeRequest, PaymentResponse authorizeResponse, string transactionId)
        {
            var paymentDoc = new CreatePaymentDocumentRequest
            {
                TransactionId = transactionId,
                DocType = Constants.Application.DocType,
                CreatedBy = Constants.Application.CreatedBy,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                ModifiedBy = string.Empty,
                ModifiedOn = string.Empty,
                BookingReferences = authorizeRequest.BookingReferences,
                CustomerDetails = authorizeRequest.CustomerDetails,
                Parties = authorizeRequest.Parties,
                Pos = authorizeRequest.Pos,
                PaymentDetails = authorizeRequest.PaymentDetails,
                JourneyType = authorizeRequest.JourneyType,
                Segments = authorizeRequest.Segments,
                OrderId = authorizeRequest.OrderId,
                Errors = authorizeResponse.Errors,
                Status = (authorizeResponse.Errors != null && authorizeResponse.Errors.Count() > 0) ? "Payment Not Authorized" : authorizeResponse.TransactionStatus
            };

            var cardDetails = authorizeRequest.PaymentDetails.First().CardMethod;
            cardDetails.CardNumber = $"{cardDetails.CardNumber.Substring(0, 6)}XXXXXX{cardDetails.CardNumber.Substring(12, 4)}";
            cardDetails.Cvv = null;
            cardDetails.ExpiryDate = null;

            paymentDoc.PaymentDetails = authorizeRequest.PaymentDetails.Select(a => new Domain.CPD.Payment()
            {
                Amount = a.Amount,
                CardMethod = cardDetails,
                Payer = new PaymentPayer()
                { 
                    FirstName = a.Payer.FirstName,
                    LastName = a.Payer.LastName
                }
            });

            paymentDoc.CustomerDetails = paymentDoc.CustomerDetails.Select(c => { c.DateOfBirth = null; return c; });

            return paymentDoc;
        }
    }
}
