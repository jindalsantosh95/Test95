using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.ReferenceMS;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Services.ReferenceMS
{
    public class AirportDetailsQueryHandler : IAirportDetailsQuery
    {
        private readonly IHttpConnector _httpConnector;

        public AirportDetailsQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<AirportDetail>> Handle()
        {
            var headers = new Dictionary<string, string>()
            {
                {"Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ReferenceMSSubscriptionKey)},
            };
            string uri = ConfigReader.Get(Constants.Environment.ReferenceMSUrl) + Constants.Application.AirportDetailsEndpoint;

            var httpResponseMessage = await _httpConnector.GetAsync(uri, headers, string.Empty);

            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<IEnumerable<AirportDetail>>(response);
            else
                return null;
        }
    }
}
