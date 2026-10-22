using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;
using VAA.EAPI.Domain.Entities.Common;


namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Services.IFG
{
    public class PaymenyOrderRefundCommandHandler : IPaymentOrderRefundCommand
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<PaymenyOrderRefundCommandHandler> _logger;

        public PaymenyOrderRefundCommandHandler(ILogger<PaymenyOrderRefundCommandHandler> logger, IHttpConnector httpConnector)
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

                var jsonPayload = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });

                var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.IFGRefundRequestUrl), headers, string.Empty, HttpMethod.Post, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                var PaymentAuthorizeResponse = new PaymentAuthorizeResponse();
                PaymentAuthorizeResponse = Json.Deserialize<PaymentAuthorizeResponse>(response);

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return new Response<PaymentAuthorizeResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.OK,
                        Value = PaymentAuthorizeResponse,
                        IgnoreNullValues = false,
                    };

                }
                else
                    return new Response<PaymentAuthorizeResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.BadRequest,
                        Value = PaymentAuthorizeResponse,
                        IgnoreNullValues = false,
                    };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in IFG OrderRefundPaymentService call - {ex.Message}");
                return null;
            }
        }

    }
}
