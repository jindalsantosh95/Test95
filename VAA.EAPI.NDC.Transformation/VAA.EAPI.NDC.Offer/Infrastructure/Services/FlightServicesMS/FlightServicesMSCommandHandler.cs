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
using VAA.EAPI.NDC.Offer.Infrastructure.Model.FlightServiceMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.FlightServicesMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.FlightService
{
    public class FlightServicesMSCommandHandler : IRequestHandler<FlightServicesMSCommand, FlightServiceResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public FlightServicesMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<FlightServiceResponse> Handle(FlightServicesMSCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.FlightServicesMSSubscriptionKey)},
                {"Channelid",Constants.Application.NDC }
            };

            using var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.FlightServicesMSUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<FlightServiceResponse>(responseStream, cancellationToken);
            else
                return null;

        }

    }

}
