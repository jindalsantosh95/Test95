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
    public class GetTransactionStatusQueryHandler : IRequestHandler<GetTransactionStatusQuery, Result<Response<GetTransactionStatusResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly ILogger<CapturePaymentCommandHandler> _logger;

        public GetTransactionStatusQueryHandler(IMediator mediator, ILogger<CapturePaymentCommandHandler> logger, IHttpConnector httpConnector)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Result<Response<GetTransactionStatusResponse>>> Handle(GetTransactionStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ConfigReader.Get(Constants.Environment.CPDAuthUsername)}:{ConfigReader.Get(Constants.Environment.CPDAuthPassword)}"));

                var headers = new Dictionary<string, string>()
                {
                    {"Authorization", $"Basic {credentials}"}
                };

                var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.CPDGetTransactionStatusRequestUrl), headers, string.Empty, HttpMethod.Post, new StringContent(request.TransactionStatusXmlRequest, Encoding.UTF8, "text/xml"));

                var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                var GetTransactionResponse = Xml.Deserialize<GetTransactionStatusResponse>(response);

                var transactionStatus = !GetTransactionResponse.Transaction.Status.StatusMessage.Any(status => status.Id == Constants.Application.CaptureSuccessCode) ? Xml.Serialize(GetTransactionResponse.Transaction.Status) : null ;
                if (transactionStatus != null)
                {
                    _logger.LogInformation($" TransactionId : {GetTransactionResponse.Transaction.TransactionId}; GetTransactionStatus Response: {transactionStatus} ");
                }

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return new Response<GetTransactionStatusResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.OK,
                        Value = GetTransactionResponse,
                        IgnoreNullValues = false,
                    };
                }
                else
                    return new Response<GetTransactionStatusResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.BadRequest,
                        Value = GetTransactionResponse,
                        IgnoreNullValues = false,
                    };

            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in GetTransactionStatusService call - {ex.Message}");
                return null;
            }
        }
    }
}
