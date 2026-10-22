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
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderChange;
using DistributionChainRoleCodeType = VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.DistributionChainRoleCodeType;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class OrderChangeAmendApisCommandHandler : IOrderChangeAmendApisCommand
    {
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly ManageCustomerRepoCommandHandler _manageCustomerRepoCommandHandler;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ILogger<OrderChangeAmendApisCommandHandler> _logger;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public OrderChangeAmendApisCommandHandler(OrderDataMSQueryHandler orderDataMSQueryHandler, ValidationMSCommandHandler validationMSCommandHandler, Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler orderSearchMSCommandHandler, PaymentMSRetrieveQueryHandler paymentMSRetrieveQueryHandler, ManageCustomerRepoCommandHandler manageCustomerRepoCommandHandler, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, IHostEnvironment hostEnvironment, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ILogger<OrderChangeAmendApisCommandHandler> logger)
        {
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _orderSearchMSCommandHandler = orderSearchMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _paymentMSRetrieveQueryHandler = paymentMSRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _manageCustomerRepoCommandHandler = manageCustomerRepoCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderChangeRQ request, string subscriptionKey, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeEventSubject
            };

            try
            {
                analyticsEventCommand.Track("PNR", request.Request?.BookingRef?.FirstOrDefault()?.BookingID);

                analyticsEventCommand.Track("OrderID", request.Request?.Order?.OrderID);

                analyticsEventCommand.Track("RequestType", "OrderChange_AmendApis");

                var distributionChain = request?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);

                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }

                var requestValidationErrors = OrderChangeValidations.ValidateOrderChangeAmendApis<IATA_OrderViewRS>(request, _hostEnvironment);

                if (requestValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderChangeAmendApisRequestValidation");
                    analyticsEventCommand.Track("ErrorId", requestValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;
                }
                
                var orderDataRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = request.Request?.Order?.OrderID ?? $"VS932{request.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);

                var orderDocErrors = OrderChangeValidations.ValidateDbOrder(orderDataRS, _hostEnvironment);

                if (orderDocErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderDataMSValidation");
                    analyticsEventCommand.Track("ErrorId", orderDocErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderDocErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderDocErrors?.ResponseMessages);
                    return errorDetails;
                }

                var paymentStatusValidationErrors = OrderChangeValidations.ValidatePaymentStatus(orderDataRS, _hostEnvironment);

                if (paymentStatusValidationErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "PaymentStatusValidation");
                    analyticsEventCommand.Track("ErrorId", paymentStatusValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(paymentStatusValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(paymentStatusValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request) }, null, true);
                var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = subscriptionKey }, new CancellationToken());

                if (validationMSResponse.Error != null)
                {
                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSResponse, _hostEnvironment);
                    analyticsEventCommand.Track("ServiceName", "ValidationMS");
                    analyticsEventCommand.Track("ErrorId", validationMSErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(validationMSErrors?.ResponseMessages);
                    return errorDetails;
                }
                analyticsEventCommand.Track("AgencyType", validationMSResponse?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", validationMSResponse?.ValidateResponse?.AgencyDetails?.RequestOrgName);
               
                if (validationMSResponse?.ValidateResponse?.AggregatorDetails is not null)
                {
                    analyticsEventCommand.Track("AggregatorName", validationMSResponse?.ValidateResponse?.AggregatorDetails?.CompanyName);
                    analyticsEventCommand.Track("AggregatorType", validationMSResponse?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                }

                analyticsEventCommand.Track("CorporateId", validationMSResponse?.ValidateResponse?.CorporateDetails?.CorporateId);
                analyticsEventCommand.Track("PosCountry", validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode);

                var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderDataRS.OrderDocument);
                var getReservationOrderDoc = await _orderSearchMSCommandHandler.Handle(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);

                var apisValidationError = OrderChangeValidations.ValidateAPISDetails<IATA_OrderViewRS>(request, getReservationOrderDoc, _hostEnvironment);

                if (apisValidationError != null)
                {
                    analyticsEventCommand.Track("ServiceName", "APISValidation");
                    analyticsEventCommand.Track("ErrorId", apisValidationError.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(apisValidationError?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(apisValidationError?.ResponseMessages);
                    return errorDetails;
                }

                if (getReservationOrderDoc is not null)
                {

                    if (orderDataRS.OrderDocument.DataList.CustomerDetails?.FirstOrDefault()?.Contacts?.CountryDailingCode != null)
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderDataRS.OrderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                    }
                    foreach (var customer in orderDataRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
                    }

                    foreach (var customer in orderDataRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsApisAdded))
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsApisAdded = true;
                    }

                    if (orderDataRS.OrderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderDataRS.OrderDocument.FareTypeRequest;

                    foreach (var customer in orderDataRS.OrderDocument.DataList.CustomerDetails)
                    {
                        var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                        if (getReservationOrderDocCustomer != null)
                        {
                            getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                        }
                    }

                    getReservationOrderDoc.ChangeType = orderDataRS.OrderDocument.ChangeType ?? getReservationOrderDoc.ChangeType;
                    getReservationOrderDoc.ChangeDateTime = orderDataRS.OrderDocument.ChangeDateTime ?? getReservationOrderDoc.ChangeDateTime;
                    getReservationOrderDoc.Version = orderDataRS.OrderDocument.Version;
                    getReservationOrderDoc.BookingReference = orderDataRS.OrderDocument.BookingReference;

                    getReservationOrderDoc = BrandMSRequestMapper.UpdateFares(orderDataRS.OrderDocument, getReservationOrderDoc);
                    IEnumerable<Infrastructure.Model.BrandMS.BrandRS> brandResponse;

                    if (getReservationOrderDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                    {
                        var brandMsReq = BrandMSRequestMapper.MapBrandMSPTCOrderRequest(getReservationOrderDoc);
                        brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, cancellationToken);

                    }
                    else
                    {
                        var brandMsReq = BrandMSRequestMapper.MapBrandMSOrderRequest(getReservationOrderDoc);
                        brandResponse = await _brandMSCommand.Handle(brandMsReq, cancellationToken);
                    }

                    getReservationOrderDoc = BrandMSResponseMapper.MapFareComponents(getReservationOrderDoc, brandResponse, orderDataRS.OrderDocument);

                    var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };

                    await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                }
                else
                {
                    throw new Exception($"SabreOrderDoc not found for orderId: {orderDataRS?.OrderDocument?.OrderNumber?.OrderId}");
                }

                var orderManagementAmendApisRequest = ManageCustomerRequestMapper.MapOrderChangeAmendApisRequest(request, getReservationOrderDoc, true);
                var orderManagementAmendApisResponse = await _manageCustomerRepoCommandHandler.Handle(orderManagementAmendApisRequest, cancellationToken);

                if (orderManagementAmendApisResponse is null)
                {
                    analyticsEventCommand.Track("ServiceName", "orderManagementAmendApisResponse");
                    
                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;

                }

                var historyDoc = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = $"{getReservationOrderDoc.OrderNumber.OrderId}-History" }, cancellationToken);

                var updateHistoryDocument = OrderDocumentMapper.UpdateOrderHistoryWithChanges(getReservationOrderDoc, historyDoc.OrderDocument);

                var publishHistoryDoc = new UpdateOrderRequest { OrderDocument = updateHistoryDocument };

                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishHistoryDoc), cancellationToken);

                OrderDocumentMapper.UpdateOrderDocumentAmendApis(orderManagementAmendApisResponse.orderDocument, getReservationOrderDoc);

                var segmentCount = getReservationOrderDoc?.DataList?.Journey?.Where(x => x?.FlightSegmentRefIds != null)?.SelectMany(x => x.FlightSegmentRefIds)?.Distinct()?.Count();

                analyticsEventCommand.Track("SegmentCount", segmentCount);

                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(new UpdateOrderRequest { OrderDocument = getReservationOrderDoc }), cancellationToken);

                var orderchangeResponse = OrderResponseMapper.MapOrderResponse(getReservationOrderDoc, null, false, false);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderchangeResponse };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeAmendApisCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Order Change AmendApis Command Handler");
                return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure occurred in analytics logging - {ex.Message}");
            }
        }

    }
}
