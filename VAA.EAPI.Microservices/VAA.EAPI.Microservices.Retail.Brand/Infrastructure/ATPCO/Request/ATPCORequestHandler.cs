using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Brand.Constants;
using VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO;

namespace VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO.Request
{
    public class ATPCORequestHandler : IRequestHandler<ATPCORequest, Models.RouteHappy.Resp>
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static ATPCOTokenResponse _atpcoToken = null;

        private readonly ILogger<ATPCORequestHandler> _logger;
        private readonly IHttpConnector _httpConnector;

        public ATPCORequestHandler(ILogger<ATPCORequestHandler> logger, IHttpConnector httpConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Models.RouteHappy.Resp> Handle(ATPCORequest request, CancellationToken cancellationToken)
        {
            await GetToken(cancellationToken);

            HttpResponseMessage httpResponseMessage;

            var requestContent = Json.Serialize(request, false);

            try
            {
                HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, ConfigReader.Get(Constants.Environment.ATPCOUrl))
                {
                    Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
                };

                httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_atpcoToken.Token_Type, _atpcoToken.Access_Token);
                httpRequestMessage.Headers.Add("x-api-key", ConfigReader.Get(Constants.Environment.ATPCOAPIKey));

                httpResponseMessage = await _httpConnector.SendAsync(httpRequestMessage, cancellationToken);

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    var ex = new HttpResponseException(httpResponseMessage);

                    ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);
                    ex.Data.Add("Request", requestContent);
                    ex.Data.Add("Response", await httpResponseMessage.Content.ReadAsStringAsync());

                    throw ex;
                }

            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("CorrelationId"))
                    ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);

                if (!ex.Data.Contains("Request"))
                    ex.Data.Add("Request", requestContent);

                _logger.LogError(ex, "Unable to obtain ATPCO response");

                throw;
            }

            try
            {
                var response = await httpResponseMessage.Content.ReadAsAsync<Models.RouteHappy.Resp>();
                foreach (var itinerary in response.Itineraries.Where(i => i.Valid == false))
                {
                    _logger.LogError($"Itinerary not matched by ATPCO", new
                    {
                        ErrorMessages = string.Join(" / ", itinerary.Errors.Messages),
                        Itinerary = itinerary.Rq.ToJson()
                    });
                }
                return response;
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("CorrelationId"))
                    ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);

                if (!ex.Data.Contains("Request"))
                    ex.Data.Add("Request", Json.Serialize(request));

                var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                _logger.LogError(ex, "Error parsing ATPCO response:" + System.Environment.NewLine + responseContent);

                throw;
            }
        }

        private async Task GetToken(CancellationToken cancellationToken)
        {
            if (_atpcoToken?.Access_Token.IsJwtTokenValid() ?? false) return;

            try
            {
                await _semaphoreSlim.WaitAsync();

                if (_atpcoToken?.Access_Token.IsJwtTokenValid() ?? false) return;

                var request = new HttpRequestMessage(HttpMethod.Post, ConfigReader.Get(Constants.Environment.ATPCOTokenUrl))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                    {
                        {"client_id", ConfigReader.Get(Constants.Environment.ATPCOClientId)},
                        {"client_secret", ConfigReader.Get(Constants.Environment.ATPCOClientSecret)},
                    })
                };

                var httpResponseMessage = await _httpConnector.SendAsync(request, cancellationToken);

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    var ex = new HttpResponseException(httpResponseMessage);

                    ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);

                    throw ex;
                }

                _atpcoToken = await httpResponseMessage.Content.ReadAsAsync<ATPCOTokenResponse>();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
