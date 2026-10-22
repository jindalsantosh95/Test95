using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Services
{
    public class TransactionServiceHandler : ITransactionService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<TransactionServiceHandler> _logger;
        private readonly int maxRetryCount = ConfigReader.Get<int>(Constants.Environment.OrderApiRetryCount);
        private readonly int baseDelayMilliSeconds = ConfigReader.Get<int>(Constants.Environment.OrderRetryDelayMilliseconds);


        public TransactionServiceHandler(IHttpConnector httpConnector, ILogger<TransactionServiceHandler> logger) 
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TransactionServiceResponse?> Handle(TransactionServiceRequest transactionRequest)
        {
            try
            {
                var tokenRequest = new Dictionary<string, string>
                {
                    { "client_id", ConfigReader.Get(Constants.Environment.ArcTransactionClientId) },
                    { "grant_type", "client_credentials" },
                    { "client_secret", ConfigReader.Get(Constants.Environment.ArcTransactionClientSecret) },
                    { "audience", ConfigReader.Get(Constants.Environment.ArcTransactionAudience) }
                };

                var tokenResponse = await _httpConnector.PostAsync(ConfigReader.Get(Constants.Environment.ArcTransactionTokenUrl), new FormUrlEncodedContent(tokenRequest));

                var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenObj = JsonSerializer.Deserialize<JsonElement>(tokenResponseContent);

                if (!tokenObj.TryGetProperty("access_token", out var accessTokenElement))
                    return null;

                var accessToken = accessTokenElement.GetString();

                    var headers = new Dictionary<string, string>()
                    {
                        { "Authorization", $"Bearer {accessToken}" },
                    };

                HttpResponseMessage response = null;
                string responseContent = string.Empty;
                TransactionServiceResponse transactionResponse = null;

                int retryCount = 0;

                do
                {
                    response = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.ArcTransactionUrl),headers,string.Empty,HttpMethod.Post,Json.Serialize(transactionRequest));

                    responseContent = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        transactionResponse = Json.Deserialize<TransactionServiceResponse>(responseContent);
                        break;
                    }

                    retryCount++;

                    if (retryCount < maxRetryCount)
                    {
                        var delay = Math.Pow(2, retryCount - 1) * baseDelayMilliSeconds;
                        await Task.Delay(TimeSpan.FromMilliseconds(delay));
                    }

                } while (retryCount < maxRetryCount);

                return transactionResponse;

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Error occurred in TransactionServiceHandler call - {ex.Message}");
                return null;
            }

        }
    }
}
