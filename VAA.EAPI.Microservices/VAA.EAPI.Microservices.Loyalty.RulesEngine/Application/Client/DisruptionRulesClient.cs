using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Client
{
    public class DisruptionRulesClient : IDisruptionRulesClient
    {
        private readonly IDisruptionInRuleService _disruptionInRuleService;
        private readonly ILogger<DisruptionRulesClient> _logger;

        public DisruptionRulesClient(IDisruptionInRuleService disruptionInRuleService, ILogger<DisruptionRulesClient> logger)
        {
            _disruptionInRuleService = disruptionInRuleService ?? throw new ArgumentNullException(nameof(disruptionInRuleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<RulesEvaluationResponse>> EvaluateAsync(InRuleEvaluationRequest request, string requestBody, CancellationToken cancellationToken)
        {
            try
            {
                var inRuleResponse = await _disruptionInRuleService.EvaluateAsync(requestBody, cancellationToken);
                var response = new RulesEvaluationResponse
                {
                    Event = request.Event,
                    InRuleResponse = inRuleResponse,
                    OrderDetails = GetScheduleChangedOrderDetails(request)
                };

                return new Response<RulesEvaluationResponse>
                {
                    Status = Status.OK,
                    Value = response
                };
            }
            catch (HttpRequestException exception)
            {
                var correlationId = AsyncContext.Current.CorrelationId.ToString();
                _logger.LogError(exception, "InRule disruption evaluation failed for correlation ID {CorrelationId}.", correlationId);

                return new Response<RulesEvaluationResponse>
                {
                    Status = MapStatus(exception.StatusCode),
                    Errors = CreateErrorDetails(correlationId)
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException exception)
            {
                var correlationId = AsyncContext.Current.CorrelationId.ToString();
                _logger.LogError(exception, "InRule disruption evaluation timed out for correlation ID {CorrelationId}.", correlationId);

                return new Response<RulesEvaluationResponse>
                {
                    Status = Status.GatewayTimeout,
                    Errors = CreateErrorDetails(correlationId)
                };
            }
            catch (Exception exception)
            {
                var correlationId = AsyncContext.Current.CorrelationId.ToString();
                _logger.LogError(exception, "InRule disruption evaluation failed for correlation ID {CorrelationId}.", correlationId);

                return new Response<RulesEvaluationResponse>
                {
                    Status = Status.Error,
                    Errors = CreateErrorDetails(correlationId)
                };
            }
        }

        private static List<OrderDetail> GetScheduleChangedOrderDetails(InRuleEvaluationRequest request)
        {
            return request.Order.SelectMany(order => (order.DataList?.SegmentDetails ?? Enumerable.Empty<VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment>())
                .Select(segment => new OrderDetail
                {
                    OrderId = order.OrderNumber?.OrderId,
                    Rloc = order.BookingReference?.RLOC,
                    SegmentId = segment.SegmentId
                })).ToList();
        }

        private static Status MapStatus(HttpStatusCode? statusCode) => statusCode switch
        {
            null => Status.BadGateway,
            HttpStatusCode.BadRequest => Status.BadRequest,
            HttpStatusCode.Unauthorized => Status.BadGateway,
            HttpStatusCode.Forbidden => Status.BadGateway,
            HttpStatusCode.NotFound => Status.BadGateway,
            HttpStatusCode.RequestTimeout => Status.GatewayTimeout,
            HttpStatusCode.TooManyRequests => Status.TooManyRequests,
            HttpStatusCode.BadGateway => Status.BadGateway,
            HttpStatusCode.ServiceUnavailable => Status.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout => Status.GatewayTimeout,
            _ => Status.Error
        };

        private static List<ErrorDetails> CreateErrorDetails(string correlationId) =>
        [
            new ErrorDetails
            {
                Id = correlationId,
                Code = "INRULE_EVALUATION_FAILED",
                Message = "InRule disruption evaluation could not be completed."
            }
        ];
    }
}
