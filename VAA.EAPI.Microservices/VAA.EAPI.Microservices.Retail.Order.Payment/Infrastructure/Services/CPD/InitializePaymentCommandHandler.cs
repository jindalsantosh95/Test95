using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
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
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CPD
{
    public class InitializePaymentCommandHandler : IRequestHandler<InitializePaymentCommand, Result<Response<InitializePaymentResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly ILogger<InitializePaymentCommandHandler> _logger;

        public InitializePaymentCommandHandler(IMediator mediator, ILogger<InitializePaymentCommandHandler> logger, IHttpConnector httpConnector)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Result<Response<InitializePaymentResponse>>> Handle(InitializePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ConfigReader.Get(Constants.Environment.CPDAuthUsername)}:{ConfigReader.Get(Constants.Environment.CPDAuthPassword)}"));

                var headers = new Dictionary<string, string>()
                {
                    {"Authorization", $"Basic {credentials}"}
                };

                var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.CPDInitializeRequestUrl), headers, string.Empty, HttpMethod.Post, new StringContent(request.InitializePaymentXMLRequest, Encoding.UTF8, "text/xml"));

                var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                var initializePaymentResponse = Xml.Deserialize<InitializePaymentResponse>(response);

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return new Response<InitializePaymentResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.OK,
                        Value = initializePaymentResponse,
                        IgnoreNullValues = false,
                    };
                }

                else
                {
                    return new Response<InitializePaymentResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.BadRequest,
                        Value = initializePaymentResponse,
                        IgnoreNullValues = false,
                    };
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in InitializePaymentService call - {ex.Message}");
                return null;
            }
        }
    }

}
