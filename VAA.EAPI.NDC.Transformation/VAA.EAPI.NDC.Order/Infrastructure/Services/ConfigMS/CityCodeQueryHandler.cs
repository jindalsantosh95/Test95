using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Location;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS
{
    public class CityCodeQueryHandler : ICityCodeQuery
    {
        private readonly IHttpConnector _httpConnector;

        public CityCodeQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<CityCodeData> Handle(string countryCode, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ConfigurationMSSubscriptionKey)},
                {"CountryCode" , countryCode }
            };


            var httpResponseMessage = await _httpConnector.GetAsync(ConfigReader.Get(Constants.Environment.ConfigurationMSCityCodeEndpoint), headers, string.Empty);

            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<CityCodeData>(response);
            else
                return null;
        }
    }
}
