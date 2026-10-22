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
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services
{
    public class BrandMSCommandHandler : IBrandMSCommand
    {
        private readonly IHttpConnector _httpConnector;

        public BrandMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<BrandRS>> Handle(BrandMSCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.BrandMSSubscriptionKey)}
            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.BrandMSUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return [Json.Deserialize<BrandRS>(response)];
            else
            {
                try
                {
                    return [Json.Deserialize<BrandRS>(response)];
                }
                catch (Exception)
                {
                    return [new() { Errors = new string[] { response } }];
                }
            }
        }
    }
}

