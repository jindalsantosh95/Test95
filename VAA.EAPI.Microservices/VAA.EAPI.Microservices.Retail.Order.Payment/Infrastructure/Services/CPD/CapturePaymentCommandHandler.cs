using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CapturePaymentCommandHandler : IRequestHandler<CapturePaymentCommand, Result<Response<PaymentResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly ILogger<CapturePaymentCommandHandler> _logger;

        public CapturePaymentCommandHandler(IMediator mediator, ILogger<CapturePaymentCommandHandler> logger, IHttpConnector httpConnector)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Result<Response<PaymentResponse>>> Handle(CapturePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ConfigReader.Get(Constants.Environment.CPDAuthUsername)}:{ConfigReader.Get(Constants.Environment.CPDAuthPassword)}"));

                var headers = new Dictionary<string, string>()
                {
                    {"Authorization", $"Basic {credentials}"}
                };

                var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.CPDCaptureRequestUrl), headers, string.Empty, HttpMethod.Post, new StringContent(request.CapturePaymentXMLRequest, Encoding.UTF8, "text/xml"));

                var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                var capturePaymentResponse = new PaymentResponse();
                capturePaymentResponse = Xml.Deserialize<PaymentResponse>(response);
                capturePaymentResponse.Status.First().Text = httpResponseMessage.StatusCode == HttpStatusCode.Accepted ? "Payment Captured" : "Capture Declined";

                if (httpResponseMessage.StatusCode == HttpStatusCode.Accepted)
                {
                    return new Response<PaymentResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.OK,
                        Value = capturePaymentResponse,
                        IgnoreNullValues = false,
                    };

                }
                else
                    return new Response<PaymentResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.BadRequest,
                        Value = capturePaymentResponse,
                        IgnoreNullValues = false,
                    };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in CapturePaymentService call - {ex.Message}");
                return null;
            }
        }
    }
}

