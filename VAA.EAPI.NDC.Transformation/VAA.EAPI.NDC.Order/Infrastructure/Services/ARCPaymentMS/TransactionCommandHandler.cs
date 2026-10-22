using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS
{
    public class TransactionCommandHandler : ITransactionCommand
    {
        private readonly IHttpConnector _httpConnector;

        public TransactionCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<TransactionServiceResponse> Handle(TransactionServiceRequest request)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ARCPaymentMSSubscriptionKey)}
            };

            var requestUrl = $"{ConfigReader.Get(Constants.Environment.ARCPaymentMSUrl)}{Constants.Application.ARCPaymentTransactionEndpoint}";

            var httpResponseMessage = await _httpConnector.SendAsync(requestUrl, headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<TransactionServiceResponse>(response);
            else
                return new TransactionServiceResponse { ErrorDetails = Json.Deserialize<IEnumerable<ErrorDetails>>(response)};
        }
    }
}
