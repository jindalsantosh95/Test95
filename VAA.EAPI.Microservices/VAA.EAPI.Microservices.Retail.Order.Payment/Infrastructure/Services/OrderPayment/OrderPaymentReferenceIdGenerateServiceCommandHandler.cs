using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.OrderPayment
{
    public class OrderPaymentReferenceIdGenerateServiceCommandHandler : IRequestHandler<OrderPaymentReferenceIdGenerateServiceCommand, Result<Response<string>>>
    {
        private readonly ILogger<OrderPaymentReferenceIdGenerateServiceCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;
        private readonly ICachedChannel _cachedChannel;

        public OrderPaymentReferenceIdGenerateServiceCommandHandler(ILogger<OrderPaymentReferenceIdGenerateServiceCommandHandler> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator, ICachedChannel cachedChannel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
        }
        public async Task<Result<Response<string>>> Handle(OrderPaymentReferenceIdGenerateServiceCommand request, CancellationToken cancellationToken)
        {
            PaymentReferenceRequest dlPRIGenerateRequest = null;
            try
            {
                dlPRIGenerateRequest = DeltaPaymentMapper.PaymentRequestMapper(request.PaymentRequest);
                //Temporary request logging to troubleshoot issues . This will be removed later
                _logger.LogInformation($"MS Request: {Json.Serialize(request.PaymentRequest, false)}");
                _logger.LogInformation($"DL Request: {Json.Serialize(dlPRIGenerateRequest, false)}");

                var headers = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(request.Channel))
                {
                    var channelEntity = await _cachedChannel.GetChannelEntity(request.Channel);

                    if (request.PaymentRequest.IsBagsPayment && request.PaymentRequest.BagsDetails.Any() && request.Channel.Is(Constants.Application.DfMobileChannel))
                    {
                        headers = new Dictionary<string, string>
                        {
                             {"appID", channelEntity.DlApplicationId},
                             {"channelId",channelEntity.DlApplicationName},
                             {"purchasePath",channelEntity.DlPaymentPurchasePath}
                        };
                    }
                    else
                    {
                        headers = new Dictionary<string, string>
                        {
                             {"applicationId", channelEntity.DlApplicationId},
                             {"channelId",channelEntity.DlApplicationName}
                        };
                    }
                }

                else
                {
                    headers = new Dictionary<string, string>
                    {
                         {"applicationId", ConfigReader.Get(Constants.Environment.DlPaymentApiApplicationId)},
                         {"channelId", ConfigReader.Get(Constants.Environment.DlPaymentApiChannelId)}
                    };
                }

                var paymentRefId = await _apiConnector.SendAsync<string>(ConfigReader.Get(Constants.Environment.DlPaymentApiGenerateRefIdUri), headers: headers, bearerToken: await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlPaymentClientId), ConfigReader.Get(Constants.Environment.DlPaymentClientSecret)), verb: System.Net.Http.HttpMethod.Post, content: Json.Serialize(dlPRIGenerateRequest, false));

                _logger.LogInformation($"DL RS PRI: {paymentRefId}");

                return Result.Success(new Response<string>
                {
                    Status = Status.OK,
                    Value = Json.Deserialize<PaymentReferenceResponse>(paymentRefId).PaymentReferenceId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} PaymentReferenceId RQ-{Json.Serialize(DeltaPaymentMapper.MaskSensitiveData(dlPRIGenerateRequest))} Error in OrderPaymentReferenceIdGenerateServiceCommandHandler with DL Response: {ex.Data["ResponseData"]?.ToString()}");
                return Result.Failure<Response<string>>((ex.InnerException ?? ex).ToString());
            }
        }
    }
}
