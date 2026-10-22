using MediatR;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.UpdateOrderMS
{
    public class PublishOrderDocMSCommandHandler : IRequestHandler<PublishOrderDocMSCommand, string>
    {
        private readonly IHttpConnector _httpConnector;

        public PublishOrderDocMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector;
        }
        public async Task<string> Handle(PublishOrderDocMSCommand request,CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderSearchMSSubscriptionKey)}
            };
            using var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.OrderSearchMSUrl) + Constants.Environment.PublishOrderDocEndpoint, headers, string.Empty, HttpMethod.Post, Json.Serialize(request.UpdateOrderRequest));
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return response;
            else
                return null;
        }
    }
}
