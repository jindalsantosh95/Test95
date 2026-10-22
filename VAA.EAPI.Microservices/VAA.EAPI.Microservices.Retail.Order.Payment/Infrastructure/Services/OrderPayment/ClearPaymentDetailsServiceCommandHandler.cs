using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.OrderPayment
{
    public class ClearPaymentDetailsServiceCommandHandler : IRequestHandler<ClearPaymentDetailsServiceCommand, Result<Response<string>>>
    {
        private readonly ILogger<ClearPaymentDetailsServiceCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        public ClearPaymentDetailsServiceCommandHandler(ILogger<ClearPaymentDetailsServiceCommandHandler> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<Result<Response<string>>> Handle(ClearPaymentDetailsServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var headers = new Dictionary<string, string>
                {
                    {"channelId", ConfigReader.Get(Constants.Environment.DlPaymentApiChannelId)},
                    {"appId","DC" },
                    {"paymentReferenceId", request.PaymentReferenceId},
                    {"purchasePath","DFWEBBOOKING" }
                };

                var response = await _apiConnector.SendAsync<string>(ConfigReader.Get(Constants.Environment.DlPaymentApiClearDetailsUrl), headers: headers, bearerToken: await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlPaymentClientId), ConfigReader.Get(Constants.Environment.DlPaymentClientSecret)), verb: System.Net.Http.HttpMethod.Post, content: string.Empty);

                _logger.LogInformation($"PRI: {request.PaymentReferenceId}, DL Response: {Json.Serialize(response)}");

                return Result.Success(new Response<string>
                {
                    Status = Status.OK,
                    Value = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} PaymentReferenceId-{request.PaymentReferenceId} Error in ClearPaymentDetailsServiceCommandHandler with DL Response {ex.Data["ResponseData"]?.ToString()}");
                return Result.Failure<Response<string>>((ex.Data["ResponseData"] ?? ex).ToString());
            }
        }
    }
}