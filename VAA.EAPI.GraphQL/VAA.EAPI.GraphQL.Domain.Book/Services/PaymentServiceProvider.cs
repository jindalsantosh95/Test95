using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment.CoBrand;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public sealed class PaymentServiceProvider : IPaymentServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.PaymentMSOcpKey) } };

        protected readonly IHttpConnector _connector;

        public PaymentServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<PaymentRefIdResponse> OrderPaymentReferenceId(OrderPaymentRequestMs orderPayment)
        {
            string orderPaymentUri = string.Concat(ConfigReader.Try(Constants.Environment.PaymentMSUrl).TrimEnd('/'), Constants.Application.PaymentOrderReferenceIdMSUrl);

            return (await _connector.SendAsync<PaymentRefIdResponse>(orderPaymentUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(orderPayment))).Response;
        }

        public async Task<PaymentResponse> PaymentDetailsAsync(string paymentReferenceId, string paymentType)
        {
            string paymentDetailsUri = string.Concat(ConfigReader.Try(Constants.Environment.PaymentMSUrl).TrimEnd('/'), Constants.Application.PaymentDetailsMSUrl).Replace("{paymentReferenceId}", paymentReferenceId);
            Dictionary<string, string> headers = new()
            {
                {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.PaymentMSOcpKey)},
                {Constants.Application.PaymentTypeHeader, paymentType}
            };

            return (await _connector.GetAsync<PaymentResponse>(paymentDetailsUri, headers, string.Empty)).Response;
        }

        public async Task<PaymentResponse> PaymentCaptureAsync(PaymentCaputreRequest capturePaymentRequest)
        {
            string paymentCaputreUri = string.Concat(ConfigReader.Try(Constants.Environment.PaymentMSUrl).TrimEnd('/'), Constants.Application.PaymentCaptureMSUrl);
            Dictionary<string, string> headers = new()
            {
                {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.PaymentMSOcpKey)}
            };

            return (await _connector.SendAsync<PaymentResponse>(paymentCaputreUri, headers, string.Empty, HttpMethod.Post, Json.Serialize(capturePaymentRequest))).Response;
        }

        public async Task<PaymentResponse> PaymentVoidAsync(OrderPaymentVoidRequest voidRequest)
        {
            string paymentVoidUri = string.Concat(ConfigReader.Try(Constants.Environment.PaymentMSUrl).TrimEnd('/'), Constants.Application.PaymentVoidMSUrl);
            Dictionary<string, string> headers = new()
            {
                {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.PaymentMSOcpKey)}
            };

            return (await _connector.SendAsync<PaymentResponse>(paymentVoidUri, headers, string.Empty, HttpMethod.Post, Json.Serialize(voidRequest))).Response;
        }

        public async Task<SynchronyTokenMSResponse> GetSynchronyDataShareTokenAsync(SynchronyTokenMSRequest tokenRequest)
        {
            string paymentMSUrl = string.Concat(ConfigReader.Try(Constants.Environment.PaymentMSUrl).TrimEnd('/'), Constants.Application.SynchronyDataShareTokenURL);
            Dictionary<string, string> headers = new()
            {
                {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.PaymentMSOcpKey)}
            };

            return (await _connector.SendAsync<SynchronyTokenMSResponse>(paymentMSUrl, headers, string.Empty, HttpMethod.Post, Json.Serialize(tokenRequest))).Response;

        }

        public async Task<EngagementScoreMSResponse> GetEngagementScoreAsync(EngagementScoreMSRequest request)
        {
            string paymentMSUrl = string.Concat(ConfigReader.Try(Constants.Environment.PaymentMSUrl).TrimEnd('/'), Constants.Application.EngagementScoreURL);
            Dictionary<string, string> headers = new()
            {
                {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.PaymentMSOcpKey)}
            };

            return (await _connector.SendAsync<EngagementScoreMSResponse>(paymentMSUrl, headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<ApplicationStatusResponse> RetrieveApplicationStatusAsync(string synchronyToken)
        {
            string paymentMSUrl = string.Concat(ConfigReader.Try(Constants.Environment.PaymentMSUrl).TrimEnd('/'), Constants.Application.ApplicationStatusPath).Replace("{token}", synchronyToken);

            Dictionary<string, string> headers = new()
            {
                {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.PaymentMSOcpKey)}
            };

            return (await _connector.GetAsync<ApplicationStatusResponse>(paymentMSUrl, headers, string.Empty)).Response;
        }
    }
}
