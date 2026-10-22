using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS
{
    public class BrandMSCommandHandler : IBrandMSCommand
    {
        private readonly IHttpConnector _httpConnector;

        public BrandMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<BrandRS>> Handle(BrandMSCommand brandReq, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.BrandMSSubscriptionKey)}
            };

            using var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.BrandMSUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(brandReq));
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return [await Json.DeserializeAsync<BrandRS>(responseStream, cancellationToken)];
            }

            else
            {
                try
                {
                    return [await Json.DeserializeAsync<BrandRS>(responseStream, cancellationToken)];
                }
                catch (Exception)
                {
                    return [new() { Errors = new string[] { httpResponseMessage.ReasonPhrase ?? httpResponseMessage.StatusCode.ToString() } }];
                }
            }

        }

    }
}

