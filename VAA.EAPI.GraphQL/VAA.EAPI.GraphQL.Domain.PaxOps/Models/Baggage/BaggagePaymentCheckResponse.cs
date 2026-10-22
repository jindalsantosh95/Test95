using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Web;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Payment;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class BaggagePaymentCheckResponse
    {
        public string CheckInId { get; set; }
        public bool? PaymentPending { get; set; }
        public string PaymentUrl { get; set; }
        public string HPPLink { get; set; }
        public string PaymentReferenceId { get; set; }
        public BaggageAmount Amount {get; set; } 

        public static async Task<BaggagePaymentCheckResponse> PaymentCheckResponseAsync(Infrastructure.Models.BaggageMS.PaymentEligibilityCheckResponse response, string existingCheckInId, bool? paymentRequired, PaymentMsResponse paymentResponse, decimal? totalPendingAmount, string currencyCode)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var PaymentReferenceId = paymentResponse != null ? HttpUtility.ParseQueryString(new Uri(paymentResponse?.HPPLink).Query).Get("paymentReferenceId") : null;

            var queryParams = new Dictionary<string, string?>
            {
                ["pri"] = PaymentReferenceId,
                ["channelId"] = Common.Constants.Application.BagsChannelId,
                ["appId"] = Common.Constants.Application.BagsAppId,
                ["purchasePath"] = Common.Constants.Application.BagsPurchasePath,
            };

            return new BaggagePaymentCheckResponse
            {
                CheckInId = await CheckInIdHelper.CreateCheckInId(existingCheckInId, null, false, response.TravelPartyId, true),
                PaymentPending = paymentRequired,
                HPPLink = paymentResponse?.HPPLink,
                PaymentReferenceId = PaymentReferenceId,
                PaymentUrl = PaymentReferenceId.IsNullOrEmpty() ? null : QueryHelpers.AddQueryString(paymentResponse?.PaymentURL, queryParams),
                Amount = new BaggageAmount
                {
                    Currency = currencyCode,
                    TotalAmount = totalPendingAmount ?? 0
                }
            };
        }
    }
}
