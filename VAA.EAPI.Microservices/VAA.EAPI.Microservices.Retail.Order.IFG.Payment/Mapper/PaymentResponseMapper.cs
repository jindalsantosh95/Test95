using System;
using System.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Mapper
{
    public class PaymentResponseMapper
    {
        public static PaymentResponse AuthorizeResponseMapper(PaymentAuthorizeResponse paymentAuthorizeResponse)
        {
            var transactionId = string.Empty;
            var pspCode = paymentAuthorizeResponse?.PspCode?.Any(code => code?.ToString()?.Split(':').Last().Trim() == "200") == true;
            if (pspCode)
            {
                var message = paymentAuthorizeResponse.PspMessage.First().ToString();
                transactionId = message.Substring(message.IndexOf("ID: '") + 5, message.IndexOf("'", message.IndexOf("ID: '") + 5) - (message.IndexOf("ID: '") + 5));
            }
            
            if (paymentAuthorizeResponse.HttpStatus == 200)
            {
                return new Domain.PaymentResponse
                {
                    TrackId = paymentAuthorizeResponse.TrackID,
                    HttpStatus = paymentAuthorizeResponse.HttpStatus,
                    Status = paymentAuthorizeResponse.Status,
                    Type =paymentAuthorizeResponse.Type,
                    Description = paymentAuthorizeResponse.Description,
                    PaymentStatus = paymentAuthorizeResponse.PaymentStatus,
                    AmountAuthorized = paymentAuthorizeResponse.AmountAuthorized,
                    ApprovalCode = GetApprovalCode(paymentAuthorizeResponse),
                    TransactionId = transactionId
                };
            }
            else
            {
                var pspMessages = paymentAuthorizeResponse.PspMessage != null ? paymentAuthorizeResponse.PspMessage.Select(x => x.ToString().Split("VSFACADE:").Last().Trim()): null;
                var errors = ErrorMapper.MapIFGExceptionToEAPIException(ErrorCodes.IFGErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, $"IFGCode: {paymentAuthorizeResponse.Status}; IFGMessage: {paymentAuthorizeResponse.Description}; TrackId: {paymentAuthorizeResponse.TrackID} {(pspMessages != null ? $"; {pspMessages?.FirstOrDefault()}" : "")}");

                return new Domain.PaymentResponse
                {
                    Errors = errors
                };
            }
        }

        public static string GetApprovalCode(PaymentAuthorizeResponse authorizationResponse)
        {
            if (authorizationResponse?.PspMessage == null || !authorizationResponse.PspMessage.Any())
                return string.Empty;

            string pspMessage = authorizationResponse.PspMessage.First().ToString();

            var match = RegexCompiled.Match(pspMessage, @"CODE:\s*'([^']*)'");

            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
