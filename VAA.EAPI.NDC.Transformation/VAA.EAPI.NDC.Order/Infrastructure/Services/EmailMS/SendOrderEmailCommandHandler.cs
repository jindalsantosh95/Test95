using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.EmailMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS
{
    public class SendOrderEmailCommandHandler(IHttpConnector httpConnector) : ISendOrderEmailCommand
    {
        private readonly IHttpConnector _httpConnector = httpConnector;

        public async Task SendOrderEmail(SendOrderEmailRequest request)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.EmailMSSubscriptionKey)}
            };

            await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.EmailMSSendOrderEmailUrl), headers, string.Empty, HttpMethod.Post, JsonConvert.SerializeObject(request));
        }
    }
}
