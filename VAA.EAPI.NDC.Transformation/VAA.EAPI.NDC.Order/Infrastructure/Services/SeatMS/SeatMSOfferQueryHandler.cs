using MediatR;
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
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS
{
    public class SeatMSOfferQueryHandler : IRequestHandler<SeatMSOfferQuery, SeatDoc>
    {
        private readonly IHttpConnector _httpConnector;

        public SeatMSOfferQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<SeatDoc> Handle(SeatMSOfferQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.SeatMSSubscriptionKey)},
            };
            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.SeatMSofferUrl) + "?offer_id=" + $"{request.OfferID}", headers, string.Empty, HttpMethod.Get, string.Empty);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<SeatDoc>(response);
            else
                return new SeatDoc() { Error = Json.Deserialize<IEnumerable<ErrorDetails>>(response) };
        }
    }
}
