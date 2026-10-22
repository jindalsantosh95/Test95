using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OfferMSReshopCancel;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSReshopCancel;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.CommonMapper;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;
using VAA.EAPI.NDC.Offer.Validations.OrderReshopCancel;

namespace VAA.EAPI.NDC.Offer.Application.Query.OrderReshopCancel
{
    public class OrderReshopCancelQueryHandler : IRequestHandler<OrderReshopCancelQuery, Result<Response<IATA_OrderReshopRS>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderReshopCancelQueryHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IGenericDataMapper _genericDataMapper;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public OrderReshopCancelQueryHandler(IMediator mediator, ILogger<OrderReshopCancelQueryHandler> logger, IHostEnvironment hostEnvironment, IGenericDataMapper genericDataMapper, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _genericDataMapper = genericDataMapper ?? throw new ArgumentNullException(nameof(genericDataMapper));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));

        }

        public async Task<Result<Response<IATA_OrderReshopRS>>> Handle(OrderReshopCancelQuery request, CancellationToken cancellationToken)
        {
            var analyticsData = new Dictionary<string, object>();

            try
            {
                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsData["DateofRequest"] = dateOfRequest;
                analyticsData["OrderID"] = request?.OrderReshopRQ?.Request?.OrderRefID;
                analyticsData["CancelReshopRequest"] =  "Y";

                var distributionChain = request.OrderReshopRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsData["AgencyId"] = seller?.ParticipatingOrg?.OrgID;
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsData["AggregatorId"] = distributor?.ParticipatingOrg?.OrgID;
                }
                var errorsValidations = OrderReshopRequestValidations.ValidateOrderReshopRequest(request.OrderReshopRQ, _hostEnvironment);

                if (errorsValidations?.Status == Status.BusinessException || errorsValidations?.Status == Status.Error)
                {
                    analyticsData["ServiceName"] = "ValidateOrderReshopRequest";
                    analyticsData["ErrorId"] = errorsValidations?.ResponseMessages?.Errors?.First().Id;

                    return ErrorDetails(errorsValidations.ResponseMessages, analyticsData);
                }

                var ValidationRequest = OrderReshopCancelRequestValidations.ValidateOrderReshopCancelOrderRefIDRequest<ErrorType>(request.OrderReshopRQ, _hostEnvironment);

                if (ValidationRequest != null && (ValidationRequest.Status == Status.BusinessException || ValidationRequest.Status == Status.Error))
                {
                    analyticsData["ServiceName"] = "ValidationRequest";
                    analyticsData["ErrorId"] = ValidationRequest?.ResponseMessages?.Errors?.First().Id;

                    return ErrorDetails(ValidationRequest.ResponseMessages, analyticsData);
                }

                var orderReshopRQ = OrderReshopCancelRequestMapper.MapCancelReshopRq(request.OrderReshopRQ);

                var validationRS = OrderReshopCancelRequestValidations.ValidateOrderReshopCancelVaRequest<ErrorType>(orderReshopRQ, _hostEnvironment);

                if (validationRS != null && (validationRS.Status == Status.BusinessException || validationRS.Status == Status.Error))
                {
                    analyticsData["ServiceName"] = "ValidationRS";
                    analyticsData["ErrorId"] = validationRS?.ResponseMessages?.Errors?.First().Id;

                    return ErrorDetails(validationRS.ResponseMessages, analyticsData);
                }

                var orderDocument = await _mediator.Send(new OrderDataMSQuery() { OrderID = request.OrderReshopRQ.Request.OrderRefID }, cancellationToken);

                analyticsData["VS_PNR"] = orderDocument?.BookingReference?.RLOC;
                analyticsData["Delta_PNR"] = orderDocument?.BookingReference?.ExternalRecordLocator;

                if (orderDocument is null)
                {
                    var response = OrderReshopCancelRequestValidations.ValidateOrderDocument<ErrorDetails>(orderReshopRQ, orderDocument, _hostEnvironment);
                    analyticsData["ServiceName"] = "OrderDataMSQuery";
                    analyticsData["ErrorId"] = response?.ResponseMessages?.Errors?.First().Id;

                    return ErrorDetails(response.ResponseMessages, analyticsData);
                }

                bool isHoldBooking = orderDocument?.TimeLimits is not null && orderDocument.OrderItems?.Any(x => x.AccountableDocumentRefs is null) == true;

                var holdBookingValidation = OrderReshopCancelRequestValidations.ValidateHoldBooking<ErrorType>(isHoldBooking , _hostEnvironment);

                if (holdBookingValidation != null)
                {
                    analyticsData["ServiceName"] = "HoldBookingValidation";
                    analyticsData["ErrorId"] = holdBookingValidation.ResponseMessages?.Errors?.First().Id;

                    return ErrorDetails(holdBookingValidation.ResponseMessages, analyticsData);
                }

                //Validating Disruption for cancel seller-up action
                var disruptionValidation = OrderReshopCancelRequestValidations.ValidateDisruptedPnr<ErrorType>(orderDocument, _hostEnvironment);

                if (disruptionValidation != null )
                {
                    analyticsData["ServiceName"] = "DisruptionValidation";
                    analyticsData["ErrorId"] = disruptionValidation?.ResponseMessages?.Errors?.First().Id;

                    return ErrorDetails(disruptionValidation.ResponseMessages, analyticsData);
                }

                bool isCancelDisruption = orderDocument.OrderItems.Any(oi => oi.SellerFollowUpAction != null && oi.SellerFollowUpAction.Any(a => a.Is("Cancel"))) && orderDocument.DataList.SegmentDetails.Any(x => x.ScheduleChangeIndicator);

                var validationAgentForRFC = OrderReshopCancelRequestValidations.ValidateAgentForRFC<ErrorType>(orderDocument, orderReshopRQ, _hostEnvironment);

                if (validationAgentForRFC != null && (validationAgentForRFC.Status == Status.BusinessException || validationAgentForRFC.Status == Status.Error))
                {
                    analyticsData["ServiceName"] = "ValidationAgentForRFC";
                    analyticsData["ErrorId"] = validationAgentForRFC?.ResponseMessages?.Errors?.First().Id;

                    return ErrorDetails(validationAgentForRFC.ResponseMessages, analyticsData);
                }
               
                var flightSegmentStatus = OrderReshopCancelRequestValidations.ValidateSegmentDetails<ErrorType>(orderDocument, _hostEnvironment);

                if (flightSegmentStatus != null && (flightSegmentStatus.Status == Status.BusinessException || flightSegmentStatus.Status == Status.Error))
                {
                    analyticsData["ServiceName"] = "FlightSegmentStatus";
                    analyticsData["ErrorId"] = flightSegmentStatus?.ResponseMessages?.Errors?.First().Id;

                    return ErrorDetails(flightSegmentStatus.ResponseMessages, analyticsData);
                }

                var parties = OrderReshopRequestMapper.GetParties(request.OrderReshopRQ);

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(parties);

                var validationMSRS = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, cancellationToken);

                if (validationMSRS?.Error != null)
                {
                    var validation = OrderReshopCancelRequestValidations.ValidateAgency<ErrorType>(validationMSRS, _hostEnvironment);
                    analyticsData["ServiceName"] = "ValidationMSRS";
                    analyticsData["ErrorId"] = validation?.ResponseMessages?.Errors?.First().Id;

                    return ErrorDetails(validation.ResponseMessages, analyticsData);
                }

                else
                {
                    analyticsData["AgencyType"] = validationMSRS?.ValidateResponse?.AgencyDetails.RequestAgencyIdType;
                    analyticsData["AgencyName"] = validationMSRS?.ValidateResponse?.AgencyDetails.RequestOrgName;

                    if (validationMSRS?.ValidateResponse?.AggregatorDetails is not null)
                    {
                        analyticsData["AggregatorName"] = validationMSRS?.ValidateResponse?.AggregatorDetails?.CompanyName;
                        analyticsData["AggregatorType"] = validationMSRS?.ValidateResponse?.AggregatorDetails?.AgencyIdType;
                    }

                    analyticsData["CorporateId"] = validationMSRS?.ValidateResponse?.CorporateDetails?.CorporateId;
                    analyticsData["PosCountry"] = validationMSRS?.ValidateResponse?.AgencyDetails?.CountryCode;
                  
                }

                var validationDoc = OrderReshopRequestValidations.ValidateOrderRFCReshopRequestAgainstDocument<ErrorType>(orderReshopRQ, orderDocument, _hostEnvironment);

                if (validationDoc != null && (validationDoc?.Status == Status.BusinessException || validationDoc?.Status == Status.Error))
                {
                    analyticsData["ServiceName"] = "ValidationDoc";
                    analyticsData["ErrorId"] = validationDoc?.ResponseMessages?.Errors?.First().Id;
                    return ErrorDetails(validationDoc.ResponseMessages, analyticsData);
                }

                var getReservationRequest = OrderSearchMSRequestMapper.MapOrderSearchRequest(orderDocument);
                var getReservationOrderDoc = await _mediator.Send(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationRequest), cancellationToken);
				
				if(getReservationOrderDoc is not null)
                {
                    var ValidationSabreReq = OrderReshopRequestValidations.ValidateSabreOrderDocSegmentCouponStatus<ErrorDetails>(getReservationOrderDoc, _hostEnvironment);

                    if (ValidationSabreReq != null)
                    {
                        analyticsData["ServiceName"] = "ValidationSabreReq";
                        analyticsData["ErrorId"] = ValidationSabreReq?.ResponseMessages?.Errors?.First().Id;

                        return ErrorDetails(ValidationSabreReq.ResponseMessages, analyticsData);
                    }
                    getReservationOrderDoc.PointOfSale = orderDocument.PointOfSale;
                }
                else
                {
                    return new Response<IATA_OrderReshopRS>()
                    {
                        Status = Status.BusinessException,
                        Value = MapToErrorOrderReshopRS(DefaultError.DefaultErrorMesaage())
                    };
                }

                bool isUsAgent = validationMSRS.ValidateResponse?.AgencyDetails?.CountryCode == "US";
                if (isUsAgent)
                {
                    foreach (var orderItem in orderDocument.OrderItems)
                    {
                        var getReservationOrderDocOrderItem = getReservationOrderDoc.OrderItems.FirstOrDefault(x => x.OrderItemId == orderItem.OrderItemId);
                        {
                            getReservationOrderDocOrderItem.PriceDetails = orderItem.PriceDetails;
                        }
                    }
                }

                getReservationOrderDoc = OrderDataMsToSaberDocumentMapper.RetainPriceDetails(orderDocument, getReservationOrderDoc);

                var offerMSReq = OfferMSReshopCancelRequestMapper.MapRequest(getReservationOrderDoc, isCancelDisruption);
                var offerMSRes = await _mediator.Send(new OfferMSReshopCancelCommand() { OrderReshopRequest = offerMSReq }, cancellationToken);
                var reponseData = OrderReshopCancelResponseMapper.MapReshopResponse(orderDocument, offerMSRes);
                
                analyticsData["IsRfc"] = offerMSRes?.IsRfc;

                DateTime? dateOfResponse = DateTime.UtcNow;
                analyticsData["DateOfResponse"] = dateOfResponse;

                return new Response<IATA_OrderReshopRS>() { Status = Status.OK, Value = reponseData };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Order Reshop Cancel Query Handler");

                analyticsData["StatusCode"] = StatusCodes.Status422UnprocessableEntity;
                return new Response<IATA_OrderReshopRS>()
                {
                    Status = Status.BusinessException,
                    Value = MapToErrorOrderReshopRS(DefaultError.DefaultErrorMesaage())
                };
            }
            finally
            {
                var analyticsEventCommand = new TrackAnalyticsEventCommand()
                {
                    Type = nameof(TrackingAnalyticsType.Microservice),
                    Subject = Constants.Application.OrderCancelEventSubject
                };
                
                analyticsEventCommand.Track(analyticsData);
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        private Response<IATA_OrderReshopRS> ErrorDetails(ResponseMessage responseMessage, Dictionary<string, object> analyticsData)
        {
            var resultCode = ErrorCodeMapper.MapErrorResultCode(responseMessage);
            
            string statusCode = resultCode == Status.BusinessException ? Constants.Application.BusinessException : resultCode == Status.Error ? Constants.Application.Error : resultCode == Status.Unauthorized ? Constants.Application.Unauthorized : string.Empty; ;

            analyticsData["StatusCode"] = statusCode;
            analyticsData["ErrorCode"] = responseMessage.Errors.First().Code;

            DateTime? dateOfResponse = DateTime.UtcNow;
            analyticsData["DateOfResponse"] = dateOfResponse;

            return new Response<IATA_OrderReshopRS>()
            {
                Status = resultCode,
                Value = MapToErrorOrderReshopRS(responseMessage)
            };
        }

        private IATA_OrderReshopRS MapToErrorOrderReshopRS(ResponseMessage errors)
        {
            return new IATA_OrderReshopRS
            {
                Items = errors.Errors.Select(error =>
                       new ErrorType()
                       {
                           Code = error.Code,
                           DescText = error.Message,
                           LangCode = Constants.Application.LangCode,
                           TypeCode = error.StatusText,
                           TagText = error.TagText,
                           URL = error.Url
                       }).ToArray()
            };
        }
    }
}
