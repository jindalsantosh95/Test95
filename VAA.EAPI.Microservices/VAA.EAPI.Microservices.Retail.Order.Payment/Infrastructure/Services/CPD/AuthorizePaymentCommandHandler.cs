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
    public class AuthorizePaymentCommandHandler : IRequestHandler<AuthorizePaymentCommand, Result<Response<PaymentResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly ILogger<AuthorizePaymentCommandHandler> _logger;

        public AuthorizePaymentCommandHandler(IMediator mediator, ILogger<AuthorizePaymentCommandHandler> logger, IHttpConnector httpConnector)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Result<Response<PaymentResponse>>> Handle(AuthorizePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ConfigReader.Get(Constants.Environment.CPDAuthUsername)}:{ConfigReader.Get(Constants.Environment.CPDAuthPassword)}"));

                var headers = new Dictionary<string, string>()
                {
                    {"Authorization", $"Basic {credentials}"}
                };

                var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.CPDAuthorizeRequestUrl), headers, string.Empty, HttpMethod.Post, new StringContent(request.AuthorizePaymentXMLRequest, Encoding.UTF8, "text/xml"));

                var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                var authorizePaymentResponse = new PaymentResponse();
                authorizePaymentResponse = Xml.Deserialize<PaymentResponse>(response);

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return new Response<PaymentResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.OK,
                        Value = authorizePaymentResponse,
                        IgnoreNullValues = false,
                    };

                }
                else
                    return new Response<PaymentResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.BadRequest,
                        Value = authorizePaymentResponse,
                        IgnoreNullValues = false,
                    };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in AuthorizePaymentService call - {ex.Message}");
                return null;
            }
        }
    }
}

