using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS
{
    public class ARCOrderDataMSQueryHandler : IARCOrderDataMSQuery
    {
        private readonly IHttpConnector _httpConnector;

        public ARCOrderDataMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<ARCDocumentResponse> Handle(string orderId, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderDataMSSubscriptionKey)
                }
            };

            var endpoint = $"{ConfigReader.Get(Constants.Environment.OrderDataMSEndpoint)}{orderId}/{Constants.Application.ARCOrderDataEndpoint}";
            var httpResponseMessage = await _httpConnector.SendAsync(endpoint, headers, string.Empty, HttpMethod.Get, string.Empty);

            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return new ARCDocumentResponse() { ARCDocument = Json.Deserialize<ARCDocument>(response) };
            }
            else
            {
                return new ARCDocumentResponse() { Errors = Json.Deserialize<List<ErrorDetails>>(response) };
            }
        }
    }
}
