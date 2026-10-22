using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Constants;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Mapper.FlightOrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.OrderCreate
{
    public class OrderCreateServiceCommandHandler : IRequestHandler<OrderCreateServiceCommand, Result<Response<OrderResponse>>>
    {
        private readonly ILogger<OrderCreateServiceCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        public OrderCreateServiceCommandHandler(ILogger<OrderCreateServiceCommandHandler> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<Result<Response<OrderResponse>>> Handle(OrderCreateServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dlOrderRequest = DeltaOrderMapper.OrderRequestMapper(request.Request);

                var logRequest = Json.Deserialize<DlOrderRequest>(Json.Serialize(dlOrderRequest));

                _logger.LogInformation($"DL Request: {Json.Serialize(DeltaOrderMapper.MaskInfo(logRequest), false)}");

                var switchToAws = ConfigReader.Try(Constants.Environment.SwitchToAws, false);

                var headers = new Dictionary<string, string>
                {
                    { "applicationId", ConfigReader.Get(Constants.Environment.DlApiApplicationId) },
                    { "channelId", ConfigReader.Get(Constants.Environment.DlApiChannelId) },
                    { "transactionId", AsyncContext.Current.CorrelationId.ToString() }
                };

                if (switchToAws)
                {
                    headers.Add("labId", ConfigReader.Try(Constants.Environment.DlAwsOrderLabId));
                    headers.Add("Location", string.Concat(ConfigReader.Try(Constants.Environment.DlAwsOrderUri), Constants.Application.DlAwsCallbackIdPath));
                    headers.Add("Retry-After", Constants.Application.DlAwsCallbackInSecs);
                }

                var deltaToken = switchToAws ? await _air4TokenGenerator.GetToken(ConfigReader.Try(Constants.Environment.DlAwsOrderClientId), ConfigReader.Try(Constants.Environment.DlAwsOrderClientSecret)) : await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlOrderClientId), ConfigReader.Get(Constants.Environment.DlOrderClientSecret));

                if (deltaToken is { IsSuccess: false, Error: { } })
                {
                    return Result.Failure<Response<OrderResponse>>(deltaToken.Error);
                }

                var orderResponse = await _apiConnector.SendAsync<DlOrderResponse>(switchToAws ? string.Concat(ConfigReader.Try(Constants.Environment.DlAwsOrderUri), Constants.Application.DlAwsOrderPath) : ConfigReader.Get(Constants.Environment.DlOrderApiUri), headers: headers, bearerToken: deltaToken.Value?.Value, verb: HttpMethod.Post, content: Json.SerializeWithDoubleHandling(dlOrderRequest, false));

                var logResponse = Json.Deserialize<DlOrderResponse>(Json.Serialize(orderResponse));

                _logger.LogInformation($"DL Response: {Json.Serialize(DeltaOrderMapper.MaskInfo(logResponse), false)}");

                return Result.Success(new Response<OrderResponse>
                {
                    Status = Status.OK,
                    Value = DeltaOrderMapper.OrderResponseMapper(orderResponse, request.Request)
                });

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation id - {AsyncContext.Current.CorrelationId} Request Processing Terminated for OrderCreateServiceCommand with DL Response: {ex.Data["Response"]?.ToString()}");

                var errorResponse = Json.Deserialize<DeltaErrorResponse>(((HttpResponseException)ex.InnerException).Response.Content.ReadAsStringAsync(CancellationToken.None).GetAwaiter().GetResult());
                return Result.Failure<Response<OrderResponse>>(Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, Json.Serialize(errorResponse))));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation id - {AsyncContext.Current.CorrelationId} Request Processing Terminated for OrderCreateServiceCommand: {ex.Message}");
                return Result.Failure<Response<OrderResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
