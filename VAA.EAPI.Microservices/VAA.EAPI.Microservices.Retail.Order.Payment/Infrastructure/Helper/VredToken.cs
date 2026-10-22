using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper
{
    public sealed class VredToken : IVredToken
    {
        private readonly IHttpConnector _httpConnector;
        private string token;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public VredToken(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }


        public async Task<string> GetToken()
        {
            if (token.IsJwtTokenValid()) return token;

            try
            {
                await _semaphoreSlim.WaitAsync();

                if (token.IsJwtTokenValid()) return token;

                var tokenRequest = new Dictionary<string, string>
                {
                    { Constants.Application.ClientId, ConfigReader.Get(Constants.Environment.VredApiClientId) },
                    { Constants.Application.ClientSecret, ConfigReader.Get(Constants.Environment.VredApiClientSecret) },
                    { Constants.Application.GrantType, Constants.Application.ClientCredentials }
                };
                var authUri = ConfigReader.Get(Constants.Environment.VredAuthenticationUrl);

                var authResponse = await _httpConnector.PostAsync(authUri, new FormUrlEncodedContent(tokenRequest));

                if (!authResponse.IsSuccessStatusCode)
                {
                    var ex = new HttpResponseException(authResponse);
                    ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);
                    throw ex;
                }
                else
                {
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(await authResponse.Content.ReadAsStringAsync());
                    token = ((JObject)result)["access_token"].ToString();
                }

                return token;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

    }
}
