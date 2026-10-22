
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.StampFCNumToPNR;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class StampFCNumToPNRMutationsProvider : IStampFCNumToPNRMutationsProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public StampFCNumToPNRMutationsProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<StampFCNumToPNRResponse> StampingFCNumberToPNR(StampFCNumToPNRRequest request)
        {
            // Get a url
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.StampingFCNumberToPNRUrl).TrimEnd('/')}/AddFFP/StampFCNumberToPNR";

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.OrderManagementAppId));

            //Invoke Auto Stamping FC Number to PNR
            Dictionary<string, string> headers = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.OrderManagementOcpKey) }
            };

            return new StampFCNumToPNRResponse
            {
                Message = (await _connector.SendAsync<string>(uri, headers, token, HttpMethod.Patch, Json.Serialize(request))).Response
            };
        }
    }
}
