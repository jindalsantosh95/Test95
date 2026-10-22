using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain.IATAAcknowledgement;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Infrastructure.Services
{
    public class PortalOCNEndpointService(IHttpConnector httpConnector) : IPortalOCNEndpointService
    {
        private readonly IHttpConnector _httpConnector = httpConnector;

        public async Task<IATAAcknowledgement> Send(IATAOrderChangeNotifRQ request)
        {
            var credentials = $"{ConfigReader.Get(Constants.Environment.OCNPortalClientKey)}:{ConfigReader.Get(Constants.Environment.OCNPortalClientSecret)}";
            var base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

            var headers = new Dictionary<string, string>()
            {
                { "Authorization", $"Basic {base64Credentials}"},
                { "Environment" , ConfigReader.Get(Constants.Environment.OCNEnvironment)}
            };

            var httpResponse = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.OCNPortalEndpoint), headers, string.Empty, HttpMethod.Post, Xml.Serialize(request));
            var response = await httpResponse.Content.ReadAsStringAsync();
            string responseXml = response.Trim('"').Replace("\\\"", "\"");

            return Xml.Deserialize<IATAAcknowledgement>(responseXml);
        }
    }
}
