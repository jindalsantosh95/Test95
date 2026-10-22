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


namespace VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS
{
    public class BrandMSCommandHandlerPTC : IBrandMSPTCCommand
    {
        private readonly IHttpConnector _httpConnector;

        public BrandMSCommandHandlerPTC(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<BrandRS>> Handle(BrandMSCommandPTC request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.BrandMSSubscriptionKey)}
            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.BrandMSUrl) + "/ptc", headers, string.Empty, HttpMethod.Post, Json.Serialize(request));
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<IEnumerable<BrandRS>>(response);
            else
            {
                try
                {
                    return Json.Deserialize<IEnumerable<BrandRS>>(response);
                }
                catch (Exception)
                {
                    return new List<BrandRS>() { new() { Errors = new string[] { response } } };
                }
            }
        }
    }
}

