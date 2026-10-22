using MediatR;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS
{
    public class PublishOrderDocMSCommandHandler : IRequestHandler<PublishOrderDocMSCommand, Unit>
    {
        private readonly IHttpConnector _httpConnector;

        public PublishOrderDocMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector;
        }
        public async Task<Unit> Handle(PublishOrderDocMSCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderSearchMSSubscriptionKey)}
            };
            _ = _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.PublishOrderDocUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request.UpdateOrderRequest));

            return Unit.Value;
        }
    }
}
