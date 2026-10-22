using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using System.Net;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class PaymentServiceProvider(IHttpConnector connector) : IPaymentServiceProvider
    {
        private readonly IHttpConnector _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        private const string UrlConfig = "TripSettings:PaymentProvider:HolidayPayment:Url";
        private const string OcpConfig = "TripSettings:PaymentProvider:HolidayPayment:OcpKey";

        public async Task<PaymentRefIdResponseMs> PaymentReferenceId(PaymentRequestMs orderPayment)
        {
            string paymentUri = string.Concat(ConfigReader.Try(Common.Constants.Environment.PaymentMSUrl).TrimEnd('/'), Common.Constants.Application.PaymentOrderReferenceIdMSUrl);

            Dictionary<string, string> headers = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.PaymentMSOcpKey) } ,
                { AsyncContext.APIChannelHeader, $"{Constants.Application.PaymentMSDLChannelId};{Constants.Application.PaymentMSDLpurchasePathKey}={Constants.Application.PaymentMSDLpurchasePathValue}" }
            };

            return (await _connector.SendAsync<PaymentRefIdResponseMs>(paymentUri, headers, string.Empty, HttpMethod.Post, Json.Serialize(orderPayment))).Response;
        }

        public async Task<PaymentTransactionDetails> PaymentDetailsAsync(string paymentReferenceId, string paymentType)
        {
            string paymentDetailsUri = string.Concat(ConfigReader.Try(Common.Constants.Environment.PaymentMSUrl).TrimEnd('/'), Common.Constants.Application.PaymentDetailsMSUrl).Replace("{paymentReferenceId}", paymentReferenceId);

            Dictionary<string, string> headers = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.PaymentMSOcpKey) },
                { AsyncContext.APIChannelHeader, $"{Constants.Application.PaymentMSDLChannelId};{Constants.Application.PaymentMSDLpurchasePathKey}={Constants.Application.PaymentMSDLpurchasePathValue}" },
                { Common.Constants.Application.PaymentTypeHeader, paymentType }
            };

            return (await _connector.GetAsync<PaymentTransactionDetails>(paymentDetailsUri, headers, string.Empty)).Response;
        }

        public async Task<HolidayPaymentResponseMs> HolidayPaymentAsync(HolidayPaymentRequestMs request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var holidayPaymentBaseUrl = ConfigReader.Try(UrlConfig);
            if (string.IsNullOrWhiteSpace(holidayPaymentBaseUrl))
                throw new InvalidOperationException($"Missing configuration value: {UrlConfig}");

            var ocpKey = ConfigReader.Try(OcpConfig);
            if (string.IsNullOrWhiteSpace(ocpKey))
                throw new InvalidOperationException($"Missing configuration value: {OcpConfig}");

            string holidayPaymentUri = string.Concat(holidayPaymentBaseUrl.TrimEnd('/'), Common.Constants.Application.HolidayPaymentMSUrl);

            Dictionary<string, string> headers = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ocpKey },
            };

            var cookieContainer = new CookieContainer();

            var apiResponse = await _connector.SendAsync<HolidayPaymentResponseEnvelopeMs>(holidayPaymentUri, headers, string.Empty, HttpMethod.Post, Json.Serialize(request), cookieContainer);
            var functionContext = AsyncContext.Current?.Context;
            if (functionContext is not null && apiResponse.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                var cookieHeaderValue = string.Join('\n', setCookies);

                if (!functionContext.Items.TryGetValue(Common.Constants.Application.ResponseHeaderKey, out var existing)
                    || existing is not Dictionary<string, string> existingDict)
                {
                    existingDict = new Dictionary<string, string>();
                    functionContext.Items[Common.Constants.Application.ResponseHeaderKey] = existingDict;
                }

                if (existingDict.TryGetValue("Set-Cookie", out var existingCookies) && !string.IsNullOrWhiteSpace(existingCookies))
                    existingDict["Set-Cookie"] = string.Join('\n', new[] { existingCookies, cookieHeaderValue }.Where(v => !string.IsNullOrWhiteSpace(v)));
                else
                    existingDict["Set-Cookie"] = cookieHeaderValue;
            }
            var envelope = apiResponse.Response;

            var session = envelope?.PaymentSessions?.FirstOrDefault();

            if (session is null)
                throw new InvalidOperationException("Holiday payment response did not contain any paymentSessions.");

            return session;
        }

    }
}
