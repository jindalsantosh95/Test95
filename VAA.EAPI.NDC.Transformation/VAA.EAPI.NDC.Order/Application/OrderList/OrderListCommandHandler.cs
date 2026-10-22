using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain.OrderListRQ;
using VAA.EAPI.NDC.Order.Domain.OrderListRS;
using VAA.EAPI.NDC.Order.Helper;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Validations.OrderListMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderList;

namespace VAA.EAPI.NDC.Order.Application.OrderList
{
    public class OrderListCommandHandler : IOrderListCommand
    {
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly IOrderListQuery _orderListQuery;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<OrderListCommandHandler> _logger;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public OrderListCommandHandler(ValidationMSCommandHandler validationMSCommandHandler, IOrderListQuery orderListQuery, IHostEnvironment hostEnvironment, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ILogger<OrderListCommandHandler> logger)
        {
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _orderListQuery = orderListQuery ?? throw new ArgumentNullException(nameof(orderListQuery));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IATA_OrderListRS>>> Handle(IATA_OrderListRQ request, string subscriptionKey, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var nDCWarnings = new List<NDCWarning>();

            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderListEventSubject
            };

            try
            {
                analyticsEventCommand.Track("RequestType", "OrderList");

                var distributionChain = request?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);

                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }

                var requestValidationErrors = OrderListValidation.ValidateOrderListRequest<IATA_OrderListRS>(request, _hostEnvironment);

                if (requestValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "RequestValidation");
                    analyticsEventCommand.Track($"ErrorId", requestValidationErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = OrderListErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Parties() { Participants = OrderValidationMapper.GetPartiesForOrderList(request) }, null, true);
                var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = subscriptionKey }, new CancellationToken());

                if (validationMSResponse.Error != null)
                {
                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderListRS>(validationMSResponse, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "ValidationMS");
                    analyticsEventCommand.Track($"ErrorId", validationMSErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = OrderListErrorMapper.GetErrorDetails(validationMSErrors?.ResponseMessages);
                    return errorDetails;
                }

                var orderDataRS = await _orderListQuery.Handle(request, cancellationToken);

                if (!orderDataRS.Any())
                {
                    var orderDataRSErrors = OrderListMSResponseValidations.ValidateOrderDataResponse<IATA_OrderListRS>(orderDataRS, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "OrderData");
                    analyticsEventCommand.Track($"ErrorId", orderDataRSErrors?.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                    
                    LogErrorDetails(orderDataRSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = OrderListErrorMapper.GetErrorDetails(orderDataRSErrors?.ResponseMessages);
                    return errorDetails;
                }

                var orderListResponse = OrderListResponseMapper.MapOrderListResponse(orderDataRS);
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                return new Response<IATA_OrderListRS>() { Status = Status.OK, Value = orderListResponse };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in OrderList Command Handler");
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);
                return OrderListErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        private void LogErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                if (responseMessage?.Errors?.Any() != true)
                {
                    _logger.LogWarning("LogErrorDetails called with null or empty ResponseMessage.Errors");
                    return;
                }

                var resultCode = ErrorCodeMapper.MapHttpStatusCode(responseMessage);

                analyticsEventCommand.Track("StatusCode", resultCode);

                foreach (var (error, index) in responseMessage.Errors.Select((e, i) => (e, i)))
                {
                    analyticsEventCommand.Track($"ErrorCode{index + 1}", error.Code);
                }

                DateTime? dateOfResponse = DateTime.UtcNow;
                analyticsEventCommand.Track("DateOfResponse", dateOfResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure occurred in analytics logging - {ex.Message}");
            }
        }
    }
}