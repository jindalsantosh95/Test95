using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions
{
    public class UpdateVredTransactionRequest
    {
        public string RequestId { get; set; }
        public string PartnerTransactionId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerUserIdDomain { get; set; }
        public string PartnerUserId { get; set; }
        public string Type { get; set; }
        public string CallbackUrl { get; set; }
        public string Details { get; set; }
        public string TransactionPartnerCode { get; set; }

        public static explicit operator UpdateVredTransactionRequest(CancelVredTransactionRequestData cancelRequest)
        {
            return new()
            {
                RequestId = Guid.NewGuid().ToString(),
                PartnerTransactionId = cancelRequest.TransactionId,
                PartnerCode = Constants.Application.PartnerCode,
                PartnerUserIdDomain = Constants.Application.PartnerCode,
                PartnerUserId = cancelRequest.MemberNumber,
                Type = cancelRequest.ActivityType.Is(Constants.Application.AccrualType)
                        ? Constants.Application.CreditTransaction : Constants.Application.DebitTransaction,
                CallbackUrl = ConfigReader.Get(Constants.Environment.VredTransactionAPICallbackUrl),
                Details = cancelRequest.Details,
                TransactionPartnerCode = cancelRequest.PartnerCode,
            };
        }

        public static explicit operator UpdateVredTransactionRequest((UpdateTransactionPnrRequest eventRequest, UpdateTransactionPnrDetails updateDetails, CreditMetaData? earnTransactionDetails, DebitMetaData? burnTransactionDetails) source)
        {
            if (source.eventRequest == null) throw new ArgumentNullException(nameof(source.eventRequest));
            if (source.updateDetails == null) throw new ArgumentNullException(nameof(source.updateDetails));

            if (source.earnTransactionDetails != null)
            {
                source.earnTransactionDetails.BookingReference = source.updateDetails.BookingReference;
                source.earnTransactionDetails.UpdatedBy = source.eventRequest.UpdatedBy;
                source.earnTransactionDetails.UpdatedDatetime = DateTimeHelper.ConvertToUtc(DateTime.UtcNow.ToString(), Constants.Application.VRedDateFormat, Constants.Application.SCVDateFormat);
            }
            else
            {
                source.burnTransactionDetails.BookingReference = source.updateDetails.BookingReference;
                source.burnTransactionDetails.UpdatedBy = source.eventRequest.UpdatedBy;
                source.burnTransactionDetails.UpdatedDatetime = DateTimeHelper.ConvertToUtc(DateTime.UtcNow.ToString(), Constants.Application.VRedDateFormat, Constants.Application.SCVDateFormat);
            }

            return new()
            {
                RequestId = Guid.NewGuid().ToString(),
                PartnerTransactionId = source.updateDetails.TransactionId,
                PartnerCode = Constants.Application.PartnerCode,
                PartnerUserIdDomain = Constants.Application.PartnerCode,
                PartnerUserId = source.eventRequest.MemberNumber,
                Type = source.updateDetails.TransactionType.Is(Constants.Application.AccrualType) ? Constants.Application.CreditTransaction : Constants.Application.DebitTransaction,
                CallbackUrl = ConfigReader.Get(Constants.Environment.VredTransactionAPICallbackUrl),
                Details = source.updateDetails.TransactionType.Is(Constants.Application.AccrualType)
                        ? JsonConvert.SerializeObject(source.earnTransactionDetails, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            Formatting = Formatting.Indented,
                        })
                        : JsonConvert.SerializeObject(source.burnTransactionDetails, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            Formatting = Formatting.Indented,
                        }),
                TransactionPartnerCode = source.updateDetails.TransactionType.Is(Constants.Application.AccrualType) ? source.earnTransactionDetails.PartnerCode : source.burnTransactionDetails.PartnerCode
            };
        }
    }
}
