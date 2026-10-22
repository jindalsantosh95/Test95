using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Interface;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class ItaFareTypeCodeQueryHandler : IitaFareTypeCode
    {
        private readonly IHttpConnector _httpConnector;

        public ItaFareTypeCodeQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

       public async Task<IEnumerable<ItaFareTypeCodeData>> Handle(string fareTypeCode, string fareSubTypeCode, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ConfigurationMSOcpKey)},
                { Constants.Application.ItaFareTypeCode , fareTypeCode },
                { Constants.Application.ItaFareSubTypeCode , fareSubTypeCode }
            };

            using var httpResponseMessage = await _httpConnector.GetAsync(ConfigReader.Get(Constants.Environment.ConfigurationMSITAFareTypeCodeEndpoint), headers, string.Empty);

            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<IEnumerable<ItaFareTypeCodeData>>(responseStream, cancellationToken);
            else
                return null;
        }
    }
}
