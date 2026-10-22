
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Services.IFG
{
    public class PaymentOrderCommandHandler : IPaymentOrderCommand
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<PaymentOrderCommandHandler> _logger;
        private readonly int maxRetryCount = ConfigReader.Get<int>(Constants.Environment.OrderApiRetryCount);
        private readonly int baseDelayMilliSeconds = ConfigReader.Get<int>(Constants.Environment.OrderRetryDelayMilliseconds);

        public PaymentOrderCommandHandler(ILogger<PaymentOrderCommandHandler> logger, IHttpConnector httpConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Result<Response<PaymentAuthorizeResponse>>> Handle(PaymentAuthorizeRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ConfigReader.Get(Constants.Environment.IFGUsername)}:{ConfigReader.Get(Constants.Environment.IFGPassword)}"));

                var headers = new Dictionary<string, string>()
                {
                    { "Authorization", $"Basic {credentials}" },
                    { "Environment", ConfigReader.Get(Constants.Environment.IFGEnvironment) }
                };

                HttpResponseMessage httpResponseMessage = null;
                string response = string.Empty;
                PaymentAuthorizeResponse paymentAuthorizeResponse = null;

                var retryCount = 0;

                do
                {
                    httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.IFGOrderRequestUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

                    response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                    paymentAuthorizeResponse = Json.Deserialize<PaymentAuthorizeResponse>(response);

                    bool shouldRetry = httpResponseMessage.StatusCode != HttpStatusCode.OK && (httpResponseMessage.StatusCode != HttpStatusCode.BadRequest || paymentAuthorizeResponse?.Status == "RET014" || paymentAuthorizeResponse?.Status == "RET029");

                    if (!shouldRetry)
                    {
                        break;
                    }
                    
                    retryCount++;

                    // Exponential backoff for retries
                    if (retryCount < maxRetryCount)
                    {
                        var delay = Math.Pow(2, retryCount - 1) * baseDelayMilliSeconds;
                        await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
                    }

                } while (retryCount < maxRetryCount);

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return new Response<PaymentAuthorizeResponse>
                    {
                        Status = Status.OK,
                        Value = paymentAuthorizeResponse,
                        IgnoreNullValues = false,
                    };
                }
                else
                {
                    return new Response<PaymentAuthorizeResponse>
                    {
                        Status = Status.BadRequest,
                        Value = paymentAuthorizeResponse,
                        IgnoreNullValues = false,
                    };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in IFG OrderPaymentService call - {ex.Message}");
                return null;
            }
        }
    }
}

