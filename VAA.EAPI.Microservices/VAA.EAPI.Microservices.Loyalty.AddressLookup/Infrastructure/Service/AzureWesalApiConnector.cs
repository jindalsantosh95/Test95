using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Constants;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service
{
    public class AzureWesalApiConnector : IAzureWesalApiConnector
    {
        private readonly HttpClient httpClient;

        public AzureWesalApiConnector()
        {
            httpClient = new();
            httpClient.DefaultRequestHeaders.Add(Constants.Application.OcpKeyHeader, ConfigReader.Get(Environment.WesalOCPSubKey));
        }

        public async Task<string> GetAddressDetails(string request)
        {
            HttpRequestMessage httprequest = new(HttpMethod.Post, ConfigReader.Get(Environment.WesalAzureEndpoint));

            httprequest.Content = new StringContent(request, Encoding.UTF8, Constants.Application.AzureEndpointContentType);

            HttpResponseMessage responseText = await httpClient.SendAsync(httprequest);

            var resContent = await responseText.Content.ReadAsStringAsync();

            XmlDocument xmlDoc = new();

            xmlDoc.LoadXml(resContent);

            return xmlDoc.InnerText;
        }
    }
}
