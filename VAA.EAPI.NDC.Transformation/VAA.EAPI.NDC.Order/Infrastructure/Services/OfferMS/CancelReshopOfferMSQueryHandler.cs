using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS
{
    public class CancelReshopOfferMSQueryHandler : IRequestHandler<CancelReshopOfferMSQuery, CancelReshopResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public CancelReshopOfferMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<CancelReshopResponse> Handle(CancelReshopOfferMSQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OfferMSSubscriptionKey)},
                {"Channelid",Constants.Application.NDC }
            };

            var httpResponseMessage = await _httpConnector.SendAsync($"{ConfigReader.Get(Constants.Environment.CancelReshopOfferMSDocEndpoint)}{request.OfferID}", headers, string.Empty, HttpMethod.Get, string.Empty);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<CancelReshopResponse>(response);
            else
                return null;

        }
    }
}
