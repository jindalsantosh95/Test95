using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.PaymentDetails;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Payment;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class PaymentServiceProvider(IHttpConnector connector) : IPaymentServiceProvider
    {
        private readonly IHttpConnector _connector = connector ?? throw new ArgumentNullException(nameof(connector));

        private static readonly Dictionary<string, string> _headers = new() {
            { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.PaymentMSOcpKey) },
            { "channel", Common.Constants.Application.MobilePaymentChannel },
            { Common.Constants.Application.PaymentTypeHeader, PaymentType.Card.ToString() },
            { "api-channel", Common.Constants.Application.MobilePaymentChannel }
        };

        private static readonly string _paymentMsUrl = ConfigReader.Get(Common.Constants.Environment.PaymentMSUrl);

        public async Task<PaymentMsResponse> PaymentReferenceAsync(BaggagePaymentRequest request)
        {
            string paymentUri = string.Concat(_paymentMsUrl.TrimEnd('/'), Common.Constants.Application.PaymentOrderReferenceIdMSUrl);

            return (await _connector.SendAsync<PaymentMsResponse>(paymentUri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<PaymentTransactionDetails> PaymentDetailsAsync(string paymentReferenceId, string paymentType)
        {
            string paymentDetailsUri = string.Concat(_paymentMsUrl.TrimEnd('/'), Common.Constants.Application.PaymentDetailsMSUrl).Replace("{paymentReferenceId}", paymentReferenceId);


            return (await _connector.GetAsync<PaymentTransactionDetails>(paymentDetailsUri, _headers, string.Empty)).Response;
        }
    }
}