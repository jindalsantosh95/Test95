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
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRQ;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderHistory;
using DistributionChainRoleCodeType = VAA.EAPI.NDC.Order.Domain.OrderHistoryRQ.DistributionChainRoleCodeType;

namespace VAA.EAPI.NDC.Order.Application.OrderHistory
{
    public class OrderHistoryQueryHandler : IOrderHistoryQuery
    {
        private readonly ILogger<OrderHistoryQueryHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly OrderSearchMSOrderHistoryCommandHandler _orderSearchMSOrderHistoryCommandHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public OrderHistoryQueryHandler(ILogger<OrderHistoryQueryHandler> logger, IHostEnvironment hostEnvironment, OrderDataMSQueryHandler orderDataMSQueryHandler, ValidationMSCommandHandler validationMSCommandHandler, OrderSearchMSOrderHistoryCommandHandler orderHistoryMSCommandHandler, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _orderSearchMSOrderHistoryCommandHandler = orderHistoryMSCommandHandler ?? throw new ArgumentNullException(nameof(orderHistoryMSCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Result<Response<IATA_OrderHistoryRS>>> Handle(IATA_OrderHistoryRQ request, string subscriptionKey, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderHistoryEventSubject
            };
            var nDCWarnings = new List<NDCWarning>();

            try
            {
                var response = new Response<IATA_OrderHistoryRS>();
                analyticsEventCommand.Track("NDCVersion", Constants.Application.NDCVersion);

                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);

                analyticsEventCommand.Track("OrderID", request.Request.Order?.OrderID);

                var distributionChain = request?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                // OrderHistory RequestValidation
                response = OrderHistoryValidation.OrderHistoryRQValidation<IATA_OrderHistoryRS>(request, _hostEnvironment);

                if (response?.ResponseMessages is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "RequestValidation");
                    analyticsEventCommand.Track($"ErrorId", response.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(response?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapperHistory.GetErrorDetails(response?.ResponseMessages);
                    return errorDetails;
                }

                // Fetch OrderDocument
                var orderRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = request.Request.Order?.OrderID }, cancellationToken);

                // Fetch Agency Details
                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Parties() { Participants = OrderValidationMapper.GetPartiesForOrderHistory(request) }, null, true);
                var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = subscriptionKey }, cancellationToken);

                if (validationMSResponse.Error is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidationMS");
                    analyticsEventCommand.Track($"ErrorId", validationMSResponse?.Error?.FirstOrDefault()?.Id);

                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderHistoryRS>(validationMSResponse, _hostEnvironment);
                    
                    LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapperHistory.GetErrorDetails(validationMSErrors?.ResponseMessages);
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

                response = OrderHistoryValidation.OrderHistoryRQAgentValidation<IATA_OrderHistoryRS>(request, validationMSResponse, _hostEnvironment);

                if (response?.ResponseMessages is not null)
                {
                    analyticsEventCommand.Track($"ErrorId", response.ResponseMessages?.Errors?.FirstOrDefault()?.Id);
                    analyticsEventCommand.Track("ServiceName", "OrderHistoryRQAgentValidation");

                    LogErrorDetails(response?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapperHistory.GetErrorDetails(response?.ResponseMessages);
                    return errorDetails;
                }

                //Fetch HistoryOrderDocument
                var orderHistoryDocument = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = $"{request.Request.Order?.OrderID}-History" }, cancellationToken);

                // Fetch from OrderSabre
                OrderDocument updatedHistoryOrderDoc = null;
                var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderRS.OrderDocument, true);
                var getReservationOrderDoc = await _orderSearchMSOrderHistoryCommandHandler.Handle(new OrderSearchMSOrderHistoryCommand(getReservationReq), cancellationToken);

                if (getReservationOrderDoc is not null)
                {
                    if (getReservationOrderDoc.DataList.SegmentDetails.Any() && orderRS.OrderDocument.OrderStatus != "CANCELLED")
                    {
                        if (orderRS.OrderDocument.DataList.CustomerDetails.First().Contacts?.CountryDailingCode != null)
                        {
                            getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderRS.OrderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                        }

                        if (orderRS.OrderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderRS.OrderDocument.FareTypeRequest;

                        foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
                        {
                            getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
                        }

                        foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails)
                        {
                            var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                            if (getReservationOrderDocCustomer != null)
                            {
                                getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                            }
                        }

                        foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsApisAdded))
                        {
                            getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsApisAdded = true;
                        }

                        if (orderRS.OrderDocument.OrderItems.Any(x => !string.IsNullOrEmpty(x.OrderItemId) && char.IsLetter(x.OrderItemId.Last())))
                        {
                            OrderDocumentMapper.MapOrderItems(orderRS.OrderDocument, getReservationOrderDoc);
                        }

                        foreach (var orderItem in orderRS.OrderDocument.OrderItems.Where(a => a.OCNTimestamp != null))
                        {
                            getReservationOrderDoc.OrderItems.First(x => x.OrderItemId == orderItem.OrderItemId).OCNTimestamp = orderItem.OCNTimestamp;
                        }

                        getReservationOrderDoc = BrandMSRequestMapper.UpdateFares(orderRS.OrderDocument, getReservationOrderDoc);
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

                        getReservationOrderDoc = BrandMSResponseMapper.MapFareComponents(getReservationOrderDoc, brandResponse, orderRS.OrderDocument);


                        bool isCashPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                        bool isEasyPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;

                        if (isCashPayment || isEasyPayment)
                        {
                            foreach (var orderItem in getReservationOrderDoc.OrderItems)
                            {
                                orderItem.PaymentDetails = orderRS.OrderDocument.OrderItems.First().PaymentDetails;
                            }
                        }

                        getReservationOrderDoc.Version = orderRS.OrderDocument.Version;
                        getReservationOrderDoc.ChangeType = orderRS.OrderDocument.ChangeType;
                        getReservationOrderDoc.ChangeDateTime = orderRS.OrderDocument.ChangeDateTime;
                        getReservationOrderDoc.IsPostSaleSSR = orderRS.OrderDocument.IsPostSaleSSR;
                        getReservationOrderDoc.BookingReference = orderRS.OrderDocument.BookingReference;
                    }

                    updatedHistoryOrderDoc = OrderDocumentMapper.UpdateOrderHistoryDocument(orderRS.OrderDocument, orderHistoryDocument?.OrderDocument, getReservationOrderDoc);

                    getReservationOrderDoc.OrderHistory = null;

                    if (orderRS.OrderDocument.OrderStatus != "CANCELLED")
                    {
                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };
                        await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                    }

                    if (updatedHistoryOrderDoc?.OrderHistory?.Any(x => x.Warnings != null && x.Warnings.Any()) == true)
                    {
                        nDCWarnings.AddRange(OrderHistoryValidation.ValidateSSROrderItems(updatedHistoryOrderDoc, _hostEnvironment));

                        nDCWarnings.AddRange(OrderHistoryValidation.ValidatePaymentFailure(updatedHistoryOrderDoc, _hostEnvironment));
                    }

                    var publishOrderHistoryDocRequest = new UpdateOrderRequest { OrderDocument = updatedHistoryOrderDoc };
                    await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderHistoryDocRequest), cancellationToken);
                }
                else
                    throw new Exception($"OrderSearchMS call failed");


                var orderResponse = OrderHistoryResponseMapper.Map(updatedHistoryOrderDoc, nDCWarnings);
                LogOrderDetails(orderResponse, getReservationOrderDoc, analyticsEventCommand);

                return new Response<IATA_OrderHistoryRS>() { Status = Status.OK, Value = orderResponse };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Order History Query Handler");
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);
                return ErrorMapperHistory.GetErrorDetails(DefaultError.DefaultErrorMesaage());
            }
            finally
            {
                stopwatch.Stop();
                analyticsEventCommand.Track("ResponseTime", $"{Math.Round(stopwatch.Elapsed.TotalSeconds, 1)}s");
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        public void LogOrderDetails(IATA_OrderHistoryRS orderRS, OrderDocument orderDocument, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

            analyticsEventCommand.Track("Delta_PNR", orderDocument?.BookingReference?.ExternalRecordLocator);

            DateTime? dateOfResponse = DateTime.UtcNow;
            analyticsEventCommand.Track("DateOfResponse", dateOfResponse);

            analyticsEventCommand.Track("POSCountry", orderDocument?.PointOfSale?.CountryCode);
            analyticsEventCommand.Track("POSCity", orderDocument?.PointOfSale?.CityCode);

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