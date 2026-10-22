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
using VAA.EAPI.NDC.Order.Infrastructure.Model.IFGAgentFopLookUp;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp
{
    public class IFGAgentFopLookUpCommandHandler : IRequestHandler<IFGAgentFopLookUpCommand, IFGAgentFopLookUpResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public IFGAgentFopLookUpCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IFGAgentFopLookUpResponse> Handle(IFGAgentFopLookUpCommand request, CancellationToken cancellationToken)
        {

            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.IFGAgentFOPLookUpSubscriptionKey) }

            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.IFGAgentFopLookUpUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return Json.Deserialize<IFGAgentFopLookUpResponse>(response);
            }
            else 
            {
                return null;
            }
        }

       
    }

}
