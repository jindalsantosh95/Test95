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
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
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
    public class OrderChangeSplitPnrCommandHandler : IOrderChangeSplitPnrCommand
    {
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly PublishSplitOrderHistoryCommandHandler _publishSplitOrderHistory;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly SplitPnrRepoCommandHandler _splitPnrRepoCommandHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly ILogger<OrderChangeSplitPnrCommandHandler> _logger;
        private readonly ISplitPnrPaymentCommand _splitPnrPaymentCommand;

        public OrderChangeSplitPnrCommandHandler(ILogger<OrderChangeSplitPnrCommandHandler> logger, OrderDataMSQueryHandler orderDataMSQueryHandler, ValidationMSCommandHandler validationMSCommandHandler, Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler orderSearchMSCommandHandler, PaymentMSRetrieveQueryHandler paymentMSRetrieveQueryHandler, SplitPnrRepoCommandHandler splitPnrRepoCommandHandler, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, IHostEnvironment hostEnvironment, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ISplitPnrPaymentCommand splitPnrPaymentCommand, PublishSplitOrderHistoryCommandHandler publishSplitOrderHistory)
        {
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _orderSearchMSCommandHandler = orderSearchMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _paymentMSRetrieveQueryHandler = paymentMSRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _splitPnrRepoCommandHandler = splitPnrRepoCommandHandler ?? throw new ArgumentNullException(nameof(splitPnrRepoCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _splitPnrPaymentCommand = splitPnrPaymentCommand ?? throw new ArgumentNullException(nameof(splitPnrPaymentCommand));
            _publishSplitOrderHistory = publishSplitOrderHistory ?? throw new ArgumentNullException(nameof(publishSplitOrderHistory)); ;
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderChangeRQ request, AugmentationPoint augmentationPoint, string subscriptionKey, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var nDCWarnings = new List<NDCWarning>();

            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeEventSubject
            };

            try
            {
                analyticsEventCommand.Track("NDCVersion", Constants.Application.NDCVersion);

                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);

                analyticsEventCommand.Track("PNR", request.Request?.BookingRef?.FirstOrDefault()?.BookingID);

                analyticsEventCommand.Track("OrderID", request.Request?.Order?.OrderID);

                analyticsEventCommand.Track("RequestType", "OrderChange_SplitPNR");

                var distributionChain = request?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
               
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }

                var requestValidationErrors = OrderChangeValidations.ValidateOrderChangeSplitPnr<IATA_OrderViewRS>(request, _hostEnvironment);

                if (requestValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderChangeSplitPnrRequestValidation");
                    analyticsEventCommand.Track("ErrorId", requestValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                    
                    LogErrorDetails(requestValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(requestValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                var orderDataRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = request.Request?.Order?.OrderID ?? $"VS932{request.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);

                var orderDocErrors = OrderChangeValidations.ValidateOrderChangeSplitPnrDbRequest<IATA_OrderViewRS>(orderDataRS, augmentationPoint, _hostEnvironment, request);
                if (orderDocErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "SplitPnrDbRequestValidation");
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

                var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderDataRS.OrderDocument);
                var getReservationOrderDoc = await _orderSearchMSCommandHandler.Handle(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationReq), cancellationToken);

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

                    if (orderDataRS.OrderDocument.OrderItems.Any(x => !string.IsNullOrEmpty(x.OrderItemId) && char.IsLetter(x.OrderItemId.Last())))
                    {
                        OrderDocumentMapper.MapOrderItems(orderDataRS.OrderDocument, getReservationOrderDoc);
                    }

                    getReservationOrderDoc = BrandMSResponseMapper.MapFareComponents(getReservationOrderDoc, brandResponse, orderDataRS.OrderDocument);
                    
                    if(orderDataRS.OrderDocument?.Version != null)
                    {
                            getReservationOrderDoc.Version = orderDataRS.OrderDocument.Version;
                    }
                    getReservationOrderDoc.ChangeType = orderDataRS.OrderDocument?.ChangeType;
                    getReservationOrderDoc.ChangeDateTime = orderDataRS.OrderDocument?.ChangeDateTime;
                    getReservationOrderDoc.BookingReference = orderDataRS.OrderDocument.BookingReference;

                    var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };

                    await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                }
                else
                {
                    throw new Exception($"SabreOrderDoc not found for orderId: {orderDataRS?.OrderDocument?.OrderNumber?.OrderId}");
                }
    
                OrderDocumentMapper.UpdateEmailDetails(getReservationOrderDoc, request, augmentationPoint);

                var orderManagementSplitPnrRequest = SplitPnrRequestMapper.MapSplitPnrRequest(getReservationOrderDoc, augmentationPoint);
                var orderManagementSplitPnrResponse = await _splitPnrRepoCommandHandler.Handle(orderManagementSplitPnrRequest, cancellationToken);

                if (orderManagementSplitPnrResponse is null)
                {
                    analyticsEventCommand.Track("ServiceName", "orderManagementSplitPnrResponse");
                    analyticsEventCommand.Track("ErrorId", orderManagementSplitPnrResponse?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }
                var parentOrderDoc = OrderDocumentMapper.CreateParentOrderDocument(orderManagementSplitPnrRequest.SplitPnrRequest, orderManagementSplitPnrResponse?.BookingReference);

                // Fetch Split PNR Details 
                var splitPnrDetails = orderManagementSplitPnrResponse?.BookingReference?.SplitRLOC?.ChildRLOCDetails;

                var latestChildRloc = splitPnrDetails.OrderByDescending(x => DateTime.ParseExact(x.SplitDateTime, "yyyy-MM-dd hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture)).First();
                var childOrderDoc = OrderDocumentMapper.CreateChildOrderDocument(orderManagementSplitPnrRequest.SplitPnrRequest, latestChildRloc);
                
                //Fetch HistoryOrderDocument
                var historyOrderDoc = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = $"{request.Request.Order?.OrderID}-History" }, cancellationToken);

                //logging warning when history doc is null
                if (historyOrderDoc?.OrderDocument == null)
                {
                    _logger.LogWarning("History order document was not found while processing a split PNR request; an initial history document will be created.");
                    analyticsEventCommand.Track("HistoryOrderDocumentMissing", true);
                }

                //creating history document based on latest order document before split
                var historyDoc = OrderHistoryDocumentUpdateMapper.HistoryDocumentCreation(orderDataRS.OrderDocument, historyOrderDoc?.OrderDocument);

                //Adding parent and child order document to history document
                var splitHistoryDoc = OrderHistoryDocumentUpdateMapper.AddSplitHistory(historyDoc, parentOrderDoc, childOrderDoc);

                //Creating Parent and Child History Documents
                var parentHistoryOrderDoc = OrderHistoryDocumentUpdateMapper.ParentHistoryDocumentCreation(splitHistoryDoc,parentOrderDoc);
                var childHistoryOrderDocument = OrderHistoryDocumentUpdateMapper.ChildHistoryDocumentCreation(splitHistoryDoc, childOrderDoc);

                // Publishing Parent History Documents
                var publishParentHistoryOrderDoc = new UpdateOrderRequest { OrderDocument = parentHistoryOrderDoc };
                await _publishSplitOrderHistory.Handle(new PublishSplitOrderHistoryCommand( publishParentHistoryOrderDoc),cancellationToken);

                 //Publishing Child History Documents
                var publishChildHistoryOrderDoc = new UpdateOrderRequest { OrderDocument = childHistoryOrderDocument };
                await _publishSplitOrderHistory.Handle(new PublishSplitOrderHistoryCommand( publishChildHistoryOrderDoc), cancellationToken);

                //mapper for payment ms new endpoint
                var splitPnrPaymentRequest = PaymentMSRequestMapper.MapSplitPnrPaymentRequest(parentOrderDoc, childOrderDoc, orderManagementSplitPnrRequest.SplitPnrRequest.splitPaxId);
                var splitPnrPaymentResponse = await _splitPnrPaymentCommand.Handle(splitPnrPaymentRequest);

                if (splitPnrPaymentResponse is null || splitPnrPaymentResponse.Errors?.Any() == true || splitPnrPaymentResponse.PaymentDocument?.Any() != true)
                {
                    analyticsEventCommand.Track("ServiceName", "splitPnrPaymentResponse");
                    analyticsEventCommand.Track("ErrorId", splitPnrPaymentResponse?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(DefaultError.DefaultErrorMesaage(), analyticsEventCommand);
                    return ErrorMapper.GetErrorDetails(DefaultError.DefaultErrorMesaage());
                }

                OrderDocumentMapper.UpdateChildOrderDoc(childOrderDoc, splitPnrPaymentResponse.PaymentDocument);

                string approvalCode = parentOrderDoc?.OrderItems?.FirstOrDefault()?.PaymentDetails?.FirstOrDefault()?.ApprovalCode;

                var orderchangeResponse = OrderResponseMapper.MapOrderResponse(getReservationOrderDoc, null, false, false, false, false, parentOrderDoc, childOrderDoc,false,false, approvalCode);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderchangeResponse };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeSplitPnrCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Order Change SplitPNR Command Handler");
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
