
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
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Services.IFG
{
    public class PaymentAuthorizeCommandHandler : IPaymentAuthorizeCommand
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<PaymentAuthorizeCommandHandler> _logger;

        public PaymentAuthorizeCommandHandler(ILogger<PaymentAuthorizeCommandHandler> logger, IHttpConnector httpConnector)
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

                var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.IFGAuthorizeRequestUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

                var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                var paymentAuthorizeResponse = new PaymentAuthorizeResponse();
                paymentAuthorizeResponse = Json.Deserialize<PaymentAuthorizeResponse>(response);

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return new Response<PaymentAuthorizeResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.OK,
                        Value = paymentAuthorizeResponse,
                        IgnoreNullValues = false,
                    };

                }
                else
                    return new Response<PaymentAuthorizeResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.BadRequest,
                        Value = paymentAuthorizeResponse,
                        IgnoreNullValues = false,
                    };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in IFG AuthorizePaymentService call - {ex.Message}");
                return null;
            }
        }
    }
}

