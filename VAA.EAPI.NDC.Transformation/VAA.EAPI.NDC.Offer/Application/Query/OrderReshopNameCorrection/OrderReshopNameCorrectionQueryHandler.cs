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
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.UpdateOrderMS.Domain;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshopNameCorrection;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OrderReshopNameCorrection;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSReshopNameCorrection;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.UpdateOrderMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.BrandMs;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Mapper.OrderReshopNameCorrection;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;
using VAA.EAPI.NDC.Offer.Validations.OrderReshopNameCorrection;

namespace VAA.EAPI.NDC.Offer.Application.Query.OrderReshop
{
    public class OrderReshopNameCorrectionQueryHandler : IRequestHandler<OrderReshopNameCorrectionQuery, Result<Response<IATA_OrderReshopRS>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderReshopNameCorrectionQueryHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IGenericDataMapper _genericDataMapper;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        public OrderReshopNameCorrectionQueryHandler(IMediator mediator, ILogger<OrderReshopNameCorrectionQueryHandler> logger, IHostEnvironment hostEnvironment, IGenericDataMapper genericDataMapper, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _genericDataMapper = genericDataMapper ?? throw new ArgumentNullException(nameof(genericDataMapper));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));

        }

        public async Task<Result<Response<IATA_OrderReshopRS>>> Handle(OrderReshopNameCorrectionQuery request, CancellationToken cancellationToken)
        {
            var nDCWarnings = new List<NDCWarning>();
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderReshopEventSubject
            };

            try
            {
                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);
                analyticsEventCommand.Track("NameCorrectionReshopRequest", "Y");

                var distributionChain = request.OrderReshopRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                // Reshop Validations
                var errorsValidations = OrderReshopRequestValidations.ValidateOrderReshopRequest(request.OrderReshopRQ, _hostEnvironment);

                if (errorsValidations?.Status == Status.BusinessException || errorsValidations?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateOrderReshopRequest");
                    analyticsEventCommand.Track($"ErrorId", errorsValidations?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(errorsValidations.ResponseMessages, analyticsEventCommand);
                } 

                //Map Request 
                var orderReshopRQ = OrderReshopNameCorrectionRequestMapper.MapNameCorrectionRq(request.OrderReshopRQ);

                analyticsEventCommand.Track("OrderID", orderReshopRQ.OrderId);
                analyticsEventCommand.Track("AgencyType", orderReshopRQ.Parties?.Participants.First().PartyType);
                analyticsEventCommand.Track("AgencyName", orderReshopRQ.Parties?.Participants.First().Name);

                //Calling Order Doc
                var orderDocument = await _mediator.Send(new OrderDataMSQuery() { OrderID = orderReshopRQ.OrderId }, cancellationToken);
                
                analyticsEventCommand.Track($"VS_PNR", orderDocument?.BookingReference?.RLOC);
                analyticsEventCommand.Track($"Delta_PNR", orderDocument?.BookingReference?.ExternalRecordLocator);

                if (orderDocument is null)
                {
                    var response = OrderReshopNameCorrectionValidation.ValidateOrderDocument<ErrorDetails>(orderReshopRQ, orderDocument, _hostEnvironment);
                   
                    analyticsEventCommand.Track("ServiceName", "OrderDataMSQuery");
                    analyticsEventCommand.Track($"ErrorId", response?.ResponseMessages?.Errors?.First().Id);
                    return ErrorDetails(response.ResponseMessages, analyticsEventCommand);
                }

                //Validate the agency details
                var errorsInAgencyValidation = OrderReshopRequestValidations.ValidateAgentInRequest(orderDocument, request.OrderReshopRQ.DistributionChain, _hostEnvironment);
                
                if (errorsInAgencyValidation?.Status == Status.BusinessException || errorsInAgencyValidation?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateAgentInRequest");
                    analyticsEventCommand.Track($"ErrorId", errorsInAgencyValidation?.ResponseMessages?.Errors?.First().Id);
                    return ErrorDetails(errorsInAgencyValidation.ResponseMessages, analyticsEventCommand);
                }

                var parties = OrderReshopNameCorrectionRequestMapper.GetParties(request.OrderReshopRQ);
                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(parties);
                var validationMSRS = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, cancellationToken);

                if (validationMSRS?.Error != null)
                {
                    var validation = OrderReshopNameCorrectionValidation.ValidateAgency<ErrorType>(validationMSRS, _hostEnvironment);
                    analyticsEventCommand.Track("ServiceName", "ValidationMSRS");
                    analyticsEventCommand.Track($"ErrorId", validation?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(validation.ResponseMessages, analyticsEventCommand);
                }

                else
                {
                    analyticsEventCommand.Track("AgencyType", validationMSRS?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType);
                    analyticsEventCommand.Track("AgencyName", validationMSRS?.ValidateResponse?.AgencyDetails?.RequestOrgName);
                    if (validationMSRS?.ValidateResponse?.AggregatorDetails is not null)
                    {
                        analyticsEventCommand.Track("AggregatorName", validationMSRS?.ValidateResponse?.AggregatorDetails?.CompanyName);
                        analyticsEventCommand.Track("AggregatorType", validationMSRS?.ValidateResponse?.AggregatorDetails?.AgencyIdType);
                    }
                    analyticsEventCommand.Track("CorporateId", validationMSRS?.ValidateResponse?.CorporateDetails?.CorporateId);
                    analyticsEventCommand.Track("PosCountry", validationMSRS?.ValidateResponse?.AgencyDetails?.CountryCode);


                }

                // Validation Of NameCorrection Request 
                var errorsNameChangeValidations = OrderReshopNameCorrectionValidation.ValidateNameCorrectionRequest<ErrorType>(request.OrderReshopRQ, orderDocument, _hostEnvironment);

                if (errorsNameChangeValidations?.Status == Status.BusinessException || errorsNameChangeValidations?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateNameCorrectionRequest");
                    analyticsEventCommand.Track($"ErrorId", errorsNameChangeValidations?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(errorsNameChangeValidations.ResponseMessages, analyticsEventCommand);
                }

                analyticsEventCommand.Track("OrderID", orderReshopRQ.OrderId);
                analyticsEventCommand.Track("AgencyType", orderReshopRQ.Parties?.Participants.First().PartyType);
                analyticsEventCommand.Track("AgencyName", orderReshopRQ.Parties?.Participants.First().Name);

                // Calling Sabre EndPoint
                var getReservationRequest = OrderSearchMSRequestMapper.MapOrderSearchRequest(orderDocument);
                var getReservationOrderDoc = await _mediator.Send(new Infrastructure.Services.OrderSearchMS.OrderSearchMSCommand(getReservationRequest), cancellationToken);

                if (getReservationOrderDoc is not null)
                {
                    var apisWarning = OrderReshopRequestValidations.ValidateOrderReshopAPISRemovalWarnings(getReservationOrderDoc, orderReshopRQ, _hostEnvironment);

                    if (apisWarning.Any())
                    {
                        nDCWarnings.AddRange(apisWarning.Where(item => !nDCWarnings.Contains(item) && !nDCWarnings.Any(x =>x.Code == "142")));
                    }
                    if (orderDocument.DataList.CustomerDetails?.FirstOrDefault()?.Contacts?.CountryDailingCode != null)
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                    }
                    foreach (var customer in orderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
                    }

                    if (orderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderDocument.FareTypeRequest;

                    foreach (var customer in orderDocument.DataList.CustomerDetails.Where(a => a.IsApisAdded))
                    {
                        getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsApisAdded = true;
                    }

                    foreach (var customer in orderDocument.DataList.CustomerDetails)
                    {
                        var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                        if (getReservationOrderDocCustomer != null)
                        {
                            getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                        }
                    }

                    getReservationOrderDoc.ChangeType = orderDocument.ChangeType ?? getReservationOrderDoc.ChangeType;
                    getReservationOrderDoc.ChangeDateTime = orderDocument.ChangeDateTime ?? getReservationOrderDoc.ChangeDateTime;
                    getReservationOrderDoc.Version = orderDocument.Version;
                    getReservationOrderDoc.BookingReference = orderDocument.BookingReference;


                    getReservationOrderDoc = BrandMsRqMapper.UpdateFares(orderDocument, getReservationOrderDoc);
                    IEnumerable<Infrastructure.Model.BrandMS.BrandRS> brandResponse;

                    if (getReservationOrderDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
                    {
                        var brandMsReq = BrandMsRqMapper.MapBrandMSPTCOrderRequest(getReservationOrderDoc, null);
                        brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, cancellationToken);

                    }
                    else
                    {
                        var brandMsReq = BrandMsRqMapper.MapBrandMSOrderRequest(getReservationOrderDoc, null);
                        brandResponse = await _brandMSCommand.Handle(brandMsReq, cancellationToken);
                    }

                    getReservationOrderDoc = BrandMsRSMapper.MapBrandMSResponseUsingSabreOrderDoc(getReservationOrderDoc, brandResponse, orderDocument);

                    var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };
                    await _mediator.Send(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                }
                else if (orderDocument.OrderStatus.Is("Cancelled"))
                    getReservationOrderDoc = orderDocument;
                else
                    throw new Exception($"OrderSearchMS call failed");

                var nameCorrectionErrors = OrderReshopNameCorrectionValidation.ValidateNameCorrectionRules<ErrorType>(orderDocument, orderReshopRQ, _hostEnvironment);

                if (nameCorrectionErrors != null)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateNameCorrectionRules"); 
                    analyticsEventCommand.Track($"ErrorId", nameCorrectionErrors?.ResponseMessages?.Errors?.First().Id);

                    return ErrorDetails(nameCorrectionErrors.ResponseMessages, analyticsEventCommand);
                }

                //Call New Offer MS EndPoint
                var offerMSReq = OfferMSReshopNameCorrectionMapper.MapRequest(getReservationOrderDoc, orderReshopRQ);
                var offerMSRes = await _mediator.Send(new OfferMSReshopNameCorrectionCommand() { OrderReshopRequest = offerMSReq }, cancellationToken);
                var responseData = OrderReshopNameCorrectionResponseMapper.MapReshopResponse(orderDocument, offerMSRes, orderReshopRQ , nDCWarnings);

                DateTime? dateOfResponse = DateTime.UtcNow;
                analyticsEventCommand.Track("DateOfResponse", dateOfRequest);
                return new Response<IATA_OrderReshopRS>() { Status = Status.OK, Value = responseData };

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Order Reshop Name Correction Query Handler");

                analyticsEventCommand.Track("StatusCode", StatusCodes.Status422UnprocessableEntity);
                return new Response<IATA_OrderReshopRS>()
                {
                    Status = Status.BusinessException,
                    Value = MapToErrorOrderReshopRS(DefaultError.DefaultErrorMesaage())
                };
            }
            finally
            {
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        private Response<IATA_OrderReshopRS> ErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            var resultCode = ErrorCodeMapper.MapErrorResultCode(responseMessage);

            string statusCode = resultCode == Status.BusinessException ? Constants.Application.BusinessException : resultCode == Status.Error ? Constants.Application.Error : resultCode == Status.Unauthorized ? Constants.Application.Unauthorized : string.Empty; ;
            
            analyticsEventCommand.Track("StatusCode", statusCode);
            analyticsEventCommand.Track("ErrorCode", responseMessage.Errors.First().Code);

            DateTime? dateOfResponse = DateTime.UtcNow;
            analyticsEventCommand.Track("DateOfResponse", dateOfResponse);

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
