using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Services
{
    public class CurrencyConversionService(IHttpConnector httpConnector) : ICurrencyConversionService
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<string> Handle(string request)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.CurrencyConversionSubKey)}
            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.CurrencyConversionServiceUrl), headers, string.Empty, HttpMethod.Post, new StringContent(request, Encoding.UTF8, "text/xml"));

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await httpResponseMessage.Content.ReadAsStringAsync();
            else
                return null;
        }
    }
}
