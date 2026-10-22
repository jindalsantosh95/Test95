using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PNR;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PNRQueryProvider : IPNRQueryProvider
    {
        protected readonly IOAuthTokenProvider _tokenProvider;
        protected readonly IHttpConnector _connector;

        public PNRQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<RetrievePNRResponse> RetrievePNR(string pnr)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.OrderSearchMSUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.OrderSearchAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.OrderSearchOcpKey) }
                            };

            return (await _connector.GetAsync<RetrievePNRResponse>($"{uri}{Common.Constants.Application.OrderSearchMSRetrievePNREndPoint}".Replace("{PNR}", pnr), headers, token)).Response;
        }
    }
}
