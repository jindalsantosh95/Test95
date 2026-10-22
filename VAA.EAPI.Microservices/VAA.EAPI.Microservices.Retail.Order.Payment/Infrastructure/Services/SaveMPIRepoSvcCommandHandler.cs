using MediatR;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services
{
    public class SaveMPIRepoSvcCommandHandler : IRequestHandler<SaveMPIRepoSvcCommand, KeyValuePair<HttpStatusCode, string>>
    {
        private readonly IHttpConnector _apiConnector;

        private static string token = string.Empty;
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public SaveMPIRepoSvcCommandHandler(IHttpConnector apiConnector)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<KeyValuePair<HttpStatusCode, string>> Handle(SaveMPIRepoSvcCommand request, CancellationToken cancellationToken)
        {
            await GetToken();

            var headers = new Dictionary<string, string>
            {
                { Constants.Application.HeaderAccept, EAPI.Infrastructure.Constants.Constants.MediaType},
                { Constants.Application.AdapterCode,  Constants.Application.AdapterCodeValue },
                { Constants.Application.ChannelId,  Constants.Application.ChannelIdValue },
                { Constants.Application.AirlineCode,  Constants.Application.AirlineCodeValue },
                { Constants.Application.HeaderCachePar,  Constants.Application.HeaderCacheParValue }
            };

            if (!string.IsNullOrEmpty(ConfigReader.Get(Constants.Environment.Air4PaymentStagingEnvPar)))
                headers.AddRange(new Dictionary<string, string> { { Constants.Application.Air4PaymentStagingEnvParName, ConfigReader.Get(Constants.Environment.Air4PaymentStagingEnvPar) } });

            var apiResponse = await _apiConnector.SendAsync(ConfigReader.Get(Constants.Environment.DeltaMpiApiUrl), headers, token, HttpMethod.Post, Json.Serialize(request.AuthenticationRequest));

            string responseMessage = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

            return new KeyValuePair<HttpStatusCode, string>(apiResponse.StatusCode, responseMessage);
        }

        private async Task GetToken()
        {
            if (token.IsJwtTokenValid()) return;

            await semaphoreSlim.WaitAsync();

            if (token.IsJwtTokenValid()) return;

            try
            {
                var tokenRequest = new Dictionary<string, string>
                {
                    { Constants.Application.ClientId, ConfigReader.Get(Constants.Environment.DeltaMPIApiClientId) },
                    { Constants.Application.ClientSecret, ConfigReader.Get(Constants.Environment.DeltaMPIApiClientSecret) },
                    { Constants.Application.GrantType, Constants.Application.ClientCredentials }
                };

                var tokenResponse = await _apiConnector.PostAsync(ConfigReader.Get(Constants.Environment.DeltaMpiApiAuthority), new FormUrlEncodedContent(tokenRequest));

                if (tokenResponse.StatusCode == HttpStatusCode.OK)
                {
                    var response = Xml.Deserialize<DeltaPaymentApiToken>(await tokenResponse.Content.ReadAsStringAsync());

                    token = response.Access_token;
                }
                else
                    throw new ExternalServiceIntegrationException(HttpStatusCode.InternalServerError.ToString(), Constants.Application.MPITokenGenerationError);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return;
        }
    }
}
