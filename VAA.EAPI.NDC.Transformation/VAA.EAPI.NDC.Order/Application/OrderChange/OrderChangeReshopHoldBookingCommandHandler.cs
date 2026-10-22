using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Validations.OrderManagementMS;
using VAA.EAPI.NDC.Order.Mapper;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.Common;
using VAA.EAPI.NDC.Order.Validations.OrderChange;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class OrderChangeReshopHoldBookingCommandHandler : IOrderChangeReshopHoldBookingCommand
    {
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly OfferMSQueryHandler _offerMSQueryHandler;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<OrderChangeReshopHoldBookingCommandHandler> _logger;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ICurrencyConversionQuery _currencyConversionQueryHandler;
        private readonly SabreOrderManagementMSCommandHandler _sabreOrderManagementMSCommandHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly ManageCustomerRepoCommandHandler _manageCustomerRepoCommandHandler;
        private readonly AirportDetailsQueryHandler _airportDetailsQueryHandler;

        public OrderChangeReshopHoldBookingCommandHandler(ValidationMSCommandHandler validationMSCommandHandler, OfferMSQueryHandler offerMSQueryHandler, IHostEnvironment hostEnvironment, ILogger<OrderChangeReshopHoldBookingCommandHandler> logger, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, OrderDataMSQueryHandler orderDataMSQueryHandler, ICurrencyConversionQuery currencyConversionQueryHandler, SabreOrderManagementMSCommandHandler sabreOrderManagementMSCommandHandler, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ManageCustomerRepoCommandHandler manageCustomerRepoCommandHandler, AirportDetailsQueryHandler airportDetailsQueryHandler)
        {
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _offerMSQueryHandler = offerMSQueryHandler ?? throw new ArgumentNullException(nameof(offerMSQueryHandler));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _currencyConversionQueryHandler = currencyConversionQueryHandler ?? throw new ArgumentNullException(nameof(currencyConversionQueryHandler));
            _sabreOrderManagementMSCommandHandler = sabreOrderManagementMSCommandHandler ?? throw new ArgumentNullException(nameof(sabreOrderManagementMSCommandHandler));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler ?? throw new ArgumentNullException(nameof(publishOrderDocMSCommandHandler));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _manageCustomerRepoCommandHandler = manageCustomerRepoCommandHandler ?? throw new ArgumentNullException(nameof(manageCustomerRepoCommandHandler));
            _airportDetailsQueryHandler = airportDetailsQueryHandler ?? throw new ArgumentNullException(nameof(airportDetailsQueryHandler));

        }

        public async Task<Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderChangeRQ request, string subscriptionKey, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeEventSubject
            };

            var nDCWarnings = new List<NDCWarning>();

            try
            {
                string offerID = (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem == null))?.OfferRefID;
                var offerDoc = await _offerMSQueryHandler.Handle((OfferMSQuery)offerID, cancellationToken);

                var dbErrors = OrderChangeValidations.ValidateDbOffer(offerDoc, offerID, _hostEnvironment);

                if (dbErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OfferStoreValidation");
                    analyticsEventCommand.Track("ErrorId", dbErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(dbErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(dbErrors?.ResponseMessages);
                    return errorDetails;
                }

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(new Infrastructure.Model.Parties() { Participants = OrderValidationMapper.GetPartiesForOrderChange(request) }, offerDoc?.Parties?.CorporateAccount, true);
                var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = subscriptionKey }, cancellationToken);
                bool isNonUsAgent = validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode != "US";

                if (validationMSResponse.Error != null)
                {
                    var validationMSErrors = AgencyValidation.ValidateAgencyDetails<IATA_OrderViewRS>(validationMSResponse, _hostEnvironment);

                    analyticsEventCommand.Track("ServiceName", "ValidationMS");
                    analyticsEventCommand.Track("ErrorId", validationMSErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(validationMSErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(validationMSErrors?.ResponseMessages);
                    return errorDetails;
                }

                analyticsEventCommand.Track("AgencyId", validationMSResponse?.ValidateResponse?.AgencyDetails?.RequestAgencyId);
                analyticsEventCommand.Track("AgencyType", validationMSResponse?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType);
                analyticsEventCommand.Track("AgencyName", validationMSResponse?.ValidateResponse?.AgencyDetails?.RequestOrgName);
                analyticsEventCommand.Track("AggregatorId", validationMSResponse?.ValidateResponse?.AggregatorDetails?.AgencyId);
                analyticsEventCommand.Track("AggregatorName", validationMSResponse?.ValidateResponse?.AggregatorDetails?.CompanyName);
                analyticsEventCommand.Track("AggregatorType", validationMSResponse?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                analyticsEventCommand.Track("CorporateId", validationMSResponse?.ValidateResponse?.CorporateDetails?.CorporateId);
                analyticsEventCommand.Track("PosCountry", validationMSResponse.ValidateResponse?.AgencyDetails?.CountryCode);

                var offerDocValidationErrors = OrderChangeValidations.ValidateVaOrderChangeRequestOfferStore<IATA_OrderViewRS>(request, _hostEnvironment, validationMSResponse, offerDoc);

                if (offerDocValidationErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "OfferDocRequestValidation");
                    analyticsEventCommand.Track("ErrorId", offerDocValidationErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(offerDocValidationErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(offerDocValidationErrors?.ResponseMessages);
                    return errorDetails;
                }

                analyticsEventCommand.Track("offerID", offerID);
                analyticsEventCommand.Track("PosCity", offerDoc.Pos?.CityCode);

                var orderChangeRQErrors = OrderChangeValidations.ValidateEvenOrderChangeRQ<IATA_OrderViewRS>(request, offerDoc, _hostEnvironment);

                if (orderChangeRQErrors?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "EvenOrderChangeRequestValidation");
                    analyticsEventCommand.Track("ErrorId", orderChangeRQErrors.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderChangeRQErrors?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderChangeRQErrors?.ResponseMessages);
                    return errorDetails;
                }

                var offerDocErros = OrderChangeValidations.ValidateEvenOrderChangeRQAgainstOfferDoc<IATA_OrderViewRS>(request, offerDoc, _hostEnvironment);

                if (offerDocErros?.ResponseMessages != null)
                {
                    analyticsEventCommand.Track("ServiceName", "EvenOrderChangeOfferDocValidation");
                    analyticsEventCommand.Track("ErrorId", offerDocErros.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(offerDocErros?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(offerDocErros?.ResponseMessages);
                    return errorDetails;
                }

                ConcurrentDictionary<OfferDetails, IEnumerable<Infrastructure.Model.BrandMS.BrandRS>> brandMsResponseListPTC = new();

                if (offerDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                {
                    var tasks = offerDoc.Offers.Select(async offer =>
                    {
                        var brandMSRQ = BrandMSRequestMapper.MapBrandMSPTCRequest(offer, offerDoc);
                        var brandResponsePTC = await _brandMSPTCCommand.Handle(brandMSRQ, cancellationToken);
                        if (!brandMsResponseListPTC.ContainsKey(offer))
                            brandMsResponseListPTC.TryAdd(offer, brandResponsePTC);
                    });
                    await Task.WhenAll(tasks);

                    if (brandMsResponseListPTC.Values.SelectMany(x => x ?? Enumerable.Empty<Infrastructure.Model.BrandMS.BrandRS>()).Any(x => x?.Errors?.Any() == true))
                    {
                        analyticsEventCommand.Track("AncillaryStatus", false);
                    }
                }
                else
                {
                    var tasks = offerDoc.Offers.Select(async offer =>
                    {
                        var brandMSRQ = BrandMSRequestMapper.MapBrandMSRequest(offer, offerDoc);
                        var brandResponse = await _brandMSCommand.Handle(brandMSRQ, cancellationToken);
                        if (!brandMsResponseListPTC.ContainsKey(offer))
                            brandMsResponseListPTC.TryAdd(offer, brandResponse);
                    });
                    await Task.WhenAll(tasks);

                    if (brandMsResponseListPTC.Values.SelectMany(x => x ?? Enumerable.Empty<Infrastructure.Model.BrandMS.BrandRS>()).Any(x => x?.Errors?.Any() == true))
                    {
                        analyticsEventCommand.Track("AncillaryStatus", false);
                    }
                }


                int penaltyIndex = 1;
                int baggageCounter = 1;
                int serviceCounter = 1;

                if (brandMsResponseListPTC.Any())
                {
                    foreach (var brandResp in brandMsResponseListPTC)
                    {
                        BrandMSResponseMapper.UpdateOfferDoc(brandResp.Value, brandResp.Key, offerDoc, ref penaltyIndex, ref baggageCounter, ref serviceCounter);
                    }
                }

                var orderDataRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = request.Request?.Order?.OrderID ?? $"VS932{request.Request.BookingRef?.FirstOrDefault()?.BookingID}{DateTime.UtcNow:yy}" }, cancellationToken);

                if (orderDataRS is null) { analyticsEventCommand.Track("ServiceName", "OrderDataMSFailed"); }

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

                bool isReshopDisruption = orderDataRS?.OrderDocument?.OrderItems?.Any(oi => oi?.SellerFollowUpAction?.Any(x => x.Is("Reshop")) == true) == true && orderDataRS?.OrderDocument?.DataList?.SegmentDetails?.Any(x => x.ScheduleChangeIndicator) == true;

                if (!isNonUsAgent)
                {
                    var currencyExchangeValue = await _currencyConversionQueryHandler.Handle(cancellationToken);
                    OrderDocumentMapper.CurrencyConversion(offerDoc, currencyExchangeValue);
                }

                var orderManagementMSRequest = OrderChangeRequestMapper.MapOrderChangeRequest(request, orderDataRS, offerDoc, null, true, false, isReshopDisruption);
                var orderManagementMSResponse = await _sabreOrderManagementMSCommandHandler.Handle(orderManagementMSRequest, cancellationToken);

                if (orderManagementMSResponse.error is not null)
                {
                    analyticsEventCommand.Track("ServiceName", "OrderManagementMS");
                    var orderManagementMS = OrderManagementMSResponseValidation.ValidateResponses<IATA_OrderViewRS>(orderManagementMSResponse, _hostEnvironment);
                    analyticsEventCommand.Track("ErrorId", orderManagementMS.ResponseMessages?.Errors?.FirstOrDefault()?.Id);

                    LogErrorDetails(orderManagementMS?.ResponseMessages, analyticsEventCommand);
                    var errorDetails = ErrorMapper.GetErrorDetails(orderManagementMS?.ResponseMessages);
                    return errorDetails;
                }

                nDCWarnings.AddRange(OrderChangeValidations.ValidateSSROrderItems(orderManagementMSResponse.orderDocument, _hostEnvironment, nDCWarnings));

                OrderDocumentMapper.UpdateOrderItemId(orderManagementMSResponse.orderDocument, orderDataRS.OrderDocument);

                orderManagementMSResponse.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;
                orderManagementMSResponse.orderDocument.ReshopHoldBookingCounter = (orderDataRS.OrderDocument.ReshopHoldBookingCounter ?? 0) + 1;

                var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = orderManagementMSResponse.orderDocument };
                await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                OrderDocumentMapper.UpdateChangeOrderDocument(orderDataRS.OrderDocument, orderManagementMSResponse.orderDocument);

                try
                {
                    var airportDetails = await _airportDetailsQueryHandler.Handle(new AirportDetailsQuery(), cancellationToken);

                    UpdateGenderAndDOB(request, orderManagementMSResponse.orderDocument);

                    var isGSTValidationFailed = OrderChangeValidations.GSTValidationAsync(validationMSResponse, airportDetails, orderManagementMSResponse.orderDocument, null);

                    var gstValidation = OrderChangeValidations.ValidateGSTDetailsAgainstOrderDoc(orderManagementMSResponse.orderDocument, request, _hostEnvironment);
                    if (gstValidation.Any())
                    {
                        isGSTValidationFailed = true;
                        nDCWarnings.AddRange(gstValidation);
                    }

                    var gstWarnings = OrderChangeValidations.ValidateGSTDetails<NDCWarning>(request, _hostEnvironment);
                    if (gstWarnings.Any())
                    {
                        isGSTValidationFailed = true;
                        nDCWarnings.AddRange(gstWarnings);
                    }

                    nDCWarnings.AddRange(OrderChangeValidations.ValidateAPISDetails<IATA_OrderViewRS>(request, _hostEnvironment, orderManagementMSResponse.orderDocument));

                    //Update APIS , SSR and GST
                    if (request.Request.DataLists.PaxList.Any(x => x.IdentityDoc != null) || (!isGSTValidationFailed && request.Request.DataLists.PaxList.Any(x => x.Employer != null)) || (request.Request.DataLists.ContactInfoList != null && request.Request.DataLists.ContactInfoList.Any(x => x.PostalAddress != null)))
                    {
                        var orderManagementAPISRequest = ManageCustomerRequestMapper.MapOrderChangeHoldBookingRequest(request, orderManagementMSResponse.orderDocument, null, isGSTValidationFailed);
                        var orderChangeAPISResponse = await _manageCustomerRepoCommandHandler.Handle(orderManagementAPISRequest, cancellationToken);

                        if (orderChangeAPISResponse != null)
                        {
                            OrderDocumentMapper.UpdateSSRAPISGSTToOrderDoc(orderManagementMSResponse.orderDocument, orderChangeAPISResponse.orderDocument);

                            nDCWarnings.AddRange(OrderChangeValidations.ValidateSSROrderItems(orderChangeAPISResponse.orderDocument, _hostEnvironment, nDCWarnings));

                            orderManagementMSResponse.orderDocument.Version = Convert.ToInt32(orderDataRS.OrderDocument.Version) + 1;

                            if (orderChangeAPISResponse.orderDocument?.Warnings?.Any() == true)
                            {
                                orderManagementMSResponse.orderDocument.Warnings = orderChangeAPISResponse?.orderDocument?.Warnings;
                            }

                            var publishOrderDocReq = new UpdateOrderRequest { OrderDocument = orderManagementMSResponse.orderDocument };
                            await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocReq), cancellationToken);

                        }
                        else
                        {
                            analyticsEventCommand.Track("ServiceName", "orderChangeAPISResponse");
                            analyticsEventCommand.Track("ErrorId", orderChangeAPISResponse?.error?.First().Id);
                            nDCWarnings.AddRange(OrderChangeValidations.SSRTechnicalWarning(_hostEnvironment));
                        }
                    }
                }
                catch
                {
                    nDCWarnings.AddRange(OrderChangeValidations.SSRTechnicalWarning(_hostEnvironment));
                }

                var orderchangeResponse = OrderResponseMapper.MapOrderResponse(orderManagementMSResponse.orderDocument, nDCWarnings, true, true, false, false, null, null, false, false, null, false, true,isReshopDisruption: isReshopDisruption);
                LogOrderDetails(orderchangeResponse, orderManagementMSResponse.orderDocument, analyticsEventCommand);

                return new Response<IATA_OrderViewRS>() { Status = Status.OK, Value = orderchangeResponse };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
_logger.LogInformation("Order change processing was cancelled. Handler={HandlerName}", nameof(OrderChangeReshopHoldBookingCommandHandler));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in OrderChangeReshopHoldBookingCommandHandler");
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

        public void LogOrderDetails(IATA_OrderViewRS orderRS, OrderDocument orderDocument, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            try
            {
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                analyticsEventCommand.Track("PNR", orderDocument?.BookingReference?.RLOC);

                analyticsEventCommand.Track("OrderId", orderDocument?.OrderNumber?.OrderId);

                DateTime? dateOfResponse = DateTime.UtcNow;

                if (orderRS?.PaymentFunctions != null)
                {
                    foreach (var (payment, index) in orderRS?.PaymentFunctions?.Select((payment, index) => (payment, index)))
                    {
                        var paymentSummary = payment?.PaymentProcessingSummary;
                        var paymentMethod = paymentSummary?.PaymentProcessingSummaryPaymentMethod?.Item;
                        var cardPayment = paymentMethod as VAA.EAPI.NDC.Order.Domain.OrderViewRS.ReceiptPaymentCardType;
                        var offlinePayment = paymentMethod as VAA.EAPI.NDC.Order.Domain.OrderViewRS.OfflinePaymentInstructionReceiptType;

                        analyticsEventCommand.Track($"TransactionId{index + 1}", payment?.PaymentProcessingSummary?.PaymentID);
                        analyticsEventCommand.Track($"TransactionAmount{index + 1}", payment?.PaymentProcessingSummary?.Amount?.Value);
                        analyticsEventCommand.Track($"TransactionCurrencyCode{index + 1}", payment?.PaymentProcessingSummary?.Amount?.CurCode);
                        analyticsEventCommand.Track($"TransactionCardType{index + 1}", cardPayment?.CardBrandCode ?? offlinePayment?.PaymentBrandCode);
                    }
                }

                var orderResponse = (orderRS.Items as OrderViewResponseType[]).First();

                bool oneWay = orderResponse.DataLists.OriginDestList.Length == 1;
                bool roundTrip = !oneWay && orderResponse.DataLists.OriginDestList.Length == 2 && orderResponse.DataLists.OriginDestList.First().OriginCode == orderResponse.DataLists.OriginDestList.Last().DestCode && orderResponse.DataLists.OriginDestList.First().DestCode == orderResponse.DataLists.OriginDestList.Last().OriginCode;

                analyticsEventCommand.Track("OneWay", oneWay ? 'Y' : 'N');
                analyticsEventCommand.Track("RoundTrip", roundTrip ? 'Y' : 'N');
                analyticsEventCommand.Track("MultiCity", !oneWay && !roundTrip ? 'Y' : 'N');

                foreach (var (ond, index) in orderDocument.DataList.OriginDestinationAssociations.Select((ond, index) => (ond, index)))
                {
                    analyticsEventCommand.Track($"OriginDestination{index + 1}", $"{ond.OriginDestination.OriginAirport}{ond.OriginDestination.DestinationAirport}");
                    analyticsEventCommand.Track($"DepartureDate{index + 1}", ond.OriginDestination.DepartureDate);
                }

                foreach (var (ptc, index) in orderResponse.DataLists.PaxList.GroupBy(x => x.PTC).Select((ptc, index) => (ptc, index)))
                {
                    analyticsEventCommand.Track($"PaxType{index + 1}", ptc.Key);
                    analyticsEventCommand.Track($"PaxCount{index + 1}", ptc.Count());
                }

                foreach (var (ond, index) in orderDocument.DataList.OriginDestinationAssociations.Select((ond, index) => (ond, index)))
                {
                    var segRefIds = orderDocument.DataList.Journey.FirstOrDefault(x => x.JourneyId == ond.JourneyRefIds.FirstOrDefault()).FlightSegmentRefIds;
                    var priceClassRefId = orderDocument.OrderItems.First().Services.FirstOrDefault(x => segRefIds.Contains(x.SegmentRefIds.FirstOrDefault())).PriceClassRefIds.First();

                    analyticsEventCommand.Track($"O&D{index + 1}Cabin", orderDocument.DataList.PriceClass.FirstOrDefault(x => x.Id == priceClassRefId).CabinType.CabinName);
                    analyticsEventCommand.Track($"O&D{index + 1}FareBrand", orderDocument.DataList.PriceClass.FirstOrDefault(x => x.Id == priceClassRefId).ProductName);
                }

                decimal totalAmount = decimal.Zero;

                foreach (var orderItem in orderResponse.Order.First().OrderItem)
                {
                    totalAmount += Convert.ToDecimal(orderItem.Price.TotalAmount.Value);
                }

                analyticsEventCommand.Track("TotalAmount", totalAmount);

                if (orderResponse.Warning?.Any() ?? false)
                {
                    foreach (var (warning, index) in orderResponse.Warning.Select((warning, index) => (warning, index)))
                    {
                        analyticsEventCommand.Track($"WarningCode{index + 1}", warning.Code);
                        analyticsEventCommand.Track($"WarningMessage{index + 1}", warning.DescText);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure occurred in analytics logging - {ex.Message}");
            }
        }
        public void UpdateGenderAndDOB(IATA_OrderChangeRQ request, OrderDocument orderDocument)
        {
            foreach (var pax in request.Request.DataLists.PaxList)
            {
                var orderDocCust = orderDocument.DataList.CustomerDetails.First(x => x.CustId == pax.PaxID);

                if (pax.Individual == null)
                {
                    pax.Individual = new()
                    {
                        GivenName = [orderDocCust.FirstName],
                        Surname = orderDocCust.LastName,
                        Birthdate = Convert.ToDateTime(orderDocCust.DateOfBirth),
                        BirthdateSpecified = true,
                        GenderCode = Enum.Parse<Domain.OrderChangeRQ.GenderCodeType>(orderDocCust.Gender),
                        GenderCodeSpecified = true,
                        TitleName = orderDocCust.Title
                    };
                }
                else
                {
                    pax.Individual.Birthdate = Convert.ToDateTime(orderDocCust.DateOfBirth);
                    pax.Individual.BirthdateSpecified = true;
                    pax.Individual.GenderCode = Enum.Parse<Domain.OrderChangeRQ.GenderCodeType>(orderDocCust.Gender);
                    pax.Individual.GenderCodeSpecified = true;
                }
            }
        }

    }
}
