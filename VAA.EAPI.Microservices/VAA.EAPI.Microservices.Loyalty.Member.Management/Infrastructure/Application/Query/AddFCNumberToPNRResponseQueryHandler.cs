using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query
{
    public class AddFCNumberToPNRResponseQueryHandler : IRequestHandler<AddFCNumberToPNRResponseQuery, AddFCNumberToPNRQueryResponse>
    {
        private readonly IHttpConnector _httpConnector;
        private static string _accessToken = null;

        public AddFCNumberToPNRResponseQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<AddFCNumberToPNRQueryResponse> Handle(AddFCNumberToPNRResponseQuery request, CancellationToken cancellationToken)
        {
            try
            {
                string endpoint = ConfigReader.Get(Constants.Environment.OrderMgtManageSpecialServiceResponseUrl).Replace("{correlationid}", request.CorrelationId);

                await GetBearerToken();

                Dictionary<string, string> headers = new()
            {
                { Constants.Environment.OcpKeyHeader, ConfigReader.Get(Constants.Environment.LoyaltyMbrMgmtOcpKey) }
            };

                var result = await _httpConnector.GetAsync(uri: endpoint, headers: headers, bearerToken: _accessToken);

                if (result.IsSuccessStatusCode)
                    return new AddFCNumberToPNRQueryResponse { Response = Constants.Application.Success };
                else
                {
                    var response = Json.Deserialize<List<ErrorDetails>>(result.Content.ReadAsStringAsync().Result);
                    return new AddFCNumberToPNRQueryResponse { Response = Constants.Application.Faliure, ErrorMessage = response.FirstOrDefault().Message };
                }
            }
            catch (Exception ex)
            {
                return new AddFCNumberToPNRQueryResponse { Response = Constants.Application.Faliure, ErrorMessage = ex.Message };
            }
        }

        private async Task GetBearerToken()
        {
            if (_accessToken.IsJwtTokenValid()) return;

            string clientId = ConfigReader.Get(Constants.Environment.LoyaltyMbrMgmtAppId);
            string clientSecret = ConfigReader.Get(Constants.Environment.LoyaltyMbrMgmtClientSecret);
            string authority = ConfigReader.Get(Constants.Environment.AzureADEndpoint);
            string resourceId = ConfigReader.Get(Constants.Environment.OrderMgtMSAppId);

            _accessToken = await ExternalHelper.GetBearerToken(clientId, clientSecret, authority, resourceId);
        }
    }
}
