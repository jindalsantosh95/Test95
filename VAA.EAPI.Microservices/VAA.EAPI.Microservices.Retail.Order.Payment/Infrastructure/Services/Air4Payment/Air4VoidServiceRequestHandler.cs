using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment
{
    public class Air4VoidServiceRequestHandler : IRequestHandler<Air4VoidServiceRequest, Result<Response<Air4formsOfPaymentsResponse>>>
    {
        private readonly ILogger<Air4VoidServiceRequestHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        public Air4VoidServiceRequestHandler(ILogger<Air4VoidServiceRequestHandler> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<Result<Response<Air4formsOfPaymentsResponse>>> Handle(Air4VoidServiceRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var mockEnabled = ConfigReader.Get<bool>(Constants.Environment.MockVoidPaymentService);
                if (mockEnabled)
                {
                    return new Response<Air4formsOfPaymentsResponse> { Status = EAPI.Domain.Entities.Common.Status.OK, Value = Json.Deserialize<Air4formsOfPaymentsResponse>(File.ReadAllText($"{EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath}/MockResponses/VoidAir4Response.json")) };
                }

                var deltaToken = await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlPaymentClientId), ConfigReader.Get(Constants.Environment.DlPaymentClientSecret));

                var headers = new Dictionary<string, string>
                    {
                         {"paymentReferenceId", request.PaymentReferenceId},
                         {"appID", ConfigReader.Get(Constants.Environment.DlPaymentApiApplicationId)},
                         {"channelId", ConfigReader.Get(Constants.Environment.DlPaymentApiChannelId) }
                    };

                var paymentDetails = await _apiConnector.SendAsync<Air4formsOfPaymentsResponse>(ConfigReader.Get(Constants.Environment.DlPaymentApiVoidUrl), headers: headers, deltaToken, HttpMethod.Post, Json.Serialize(request));

                return new Response<Air4formsOfPaymentsResponse>
                {
                    Status = EAPI.Domain.Entities.Common.Status.OK,
                    Value = paymentDetails,
                    IgnoreNullValues = false,
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation id:{AsyncContext.Current.CorrelationId} Request Processing Terminated for Air4VoidServiceCommand");

                var errorResponse = Json.Deserialize<DeltaErrorResponse>(await ((HttpResponseException)ex.InnerException).Response.Content.ReadAsStringAsync(CancellationToken.None));
                return Result.Failure<Response<Air4formsOfPaymentsResponse>>(Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, Json.Serialize(errorResponse))));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Correlation id:{AsyncContext.Current.CorrelationId} Request Processing Terminated for Air4VoidServiceCommand");
                return Result.Failure<Response<Air4formsOfPaymentsResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation($"Correlation id:{AsyncContext.Current.CorrelationId} Executed Air4VoidServiceCommand ");
            }

        }
    }
}
