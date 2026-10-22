using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using Environment = VAA.EAPI.NDC.Order.Constants.Environment;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS
{
    public class OfferMSQueryHandler : IRequestHandler<OfferMSQuery, OfferResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public OfferMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OfferResponse> Handle(OfferMSQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Environment.OfferMSSubscriptionKey)},
                {"Channelid",Constants.Application.NDC }
            };

            var httpResponseMessage = await _httpConnector.SendAsync($"{ConfigReader.Get(Environment.OfferMSDocEndpoint)}{request.OfferID}", headers, string.Empty, HttpMethod.Get, string.Empty);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<OfferResponse>(response);
            else
                return null;

        }
    }
}
