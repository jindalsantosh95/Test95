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
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.UpdateOrderMS.Domain;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSSearchQuery;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.UpdateOrderMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.BrandMs;
using VAA.EAPI.NDC.Offer.Mapper.CommonMapper;
using VAA.EAPI.NDC.Offer.Mapper.FlightServiceMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations.OrderQuote;
using VAA.EAPI.NDC.Offer.Validations.ServiceListValidations;
using ErrorType = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRS.ErrorType;

namespace VAA.EAPI.NDC.Offer.Application.Query.ServiceList
{
    public class ServiceListQueryHandler : IRequestHandler<ServiceListQuery, Result<Response<IATA_ServiceListRS>>>
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IMediator _mediator;
        private readonly ILogger<ServiceListQueryHandler> _logger;
        private readonly IServiceListRequestValidations<IATA_ServiceListRS> _requestValidations;
        private readonly IGenericDataMapper _genericDataMapper;
        private readonly IServiceListResponseValidation _responseValidations;
        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public ServiceListQueryHandler(IMediator iMediator, IHostEnvironment hostEnvironment, ILogger<ServiceListQueryHandler> logger, IServiceListRequestValidations<IATA_ServiceListRS> requestValidations, IGenericDataMapper genericDataMapper, IServiceListResponseValidation responseValidation, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = iMediator ?? throw new ArgumentNullException(nameof(iMediator));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            _requestValidations = requestValidations;
            _responseValidations = responseValidation;
            _genericDataMapper = genericDataMapper ?? throw new ArgumentNullException(nameof(genericDataMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));

        }

        public async Task<Result<Response<IATA_ServiceListRS>>> Handle(ServiceListQuery request, CancellationToken cancellationToken)
        {
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.ServiceListEventSubject
            };


            try
            {
                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);


                bool isOfferServiceRq = request.ServiceListRQ.Request.ServiceListCoreRequest.Item is ServiceListOfferRequestType { Offer: not null };
                analyticsEventCommand.Track("IsPrime", isOfferServiceRq ? 'Y' : 'N');

                var distributionChain = request.ServiceListRQ?.DistributionChain;

                var seller = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller);
                var distributor = distributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Distributor);

                analyticsEventCommand.Track("AgencyId", seller?.ParticipatingOrg?.OrgID);
                if (distributor?.ParticipatingOrg?.OrgID is not null)
                {
                    analyticsEventCommand.Track("AggregatorId", distributor?.ParticipatingOrg?.OrgID);
                }
                var errorRequest = _requestValidations.ValidateServiceListRequest(request.ServiceListRQ, isOfferServiceRq);

                if (errorRequest?.Status == Status.BusinessException || errorRequest?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "RequestValidation"); 
                    analyticsEventCommand.Track($"ErrorId", errorRequest?.ResponseMessages?.Errors?.First().Id);
                    return ErrorDetails(errorRequest.ResponseMessages, analyticsEventCommand);
                }

                //Check Offer in request         
                OrderDocument orderDocument = null;
                OfferResponse offerDocument = null;

                if (isOfferServiceRq)
                {
                    var offerId = (request.ServiceListRQ.Request.ServiceListCoreRequest.Item as ServiceListOfferRequestType).Offer.FirstOrDefault()?.OfferID;
                    offerDocument = offerId != null ? await _mediator.Send(new OfferMSSearchQuery(offerId), cancellationToken) : null;
                    analyticsEventCommand.Track("OfferID", offerId);
                }
                else
                {
                    var orderId = (request.ServiceListRQ.Request.ServiceListCoreRequest.Item as ServiceListOrderRequestType).Order?.OrderID ?? "DefaultOrderID";
                    analyticsEventCommand.Track("OrderID", orderId);

                    orderDocument = await _mediator.Send(new OrderDataMSQuery() { OrderID = orderId }, cancellationToken);

                    var orderDocumentValidation = OrderQuoteRequestValidations.ValidateOrderQuoteHoldBookingRequest(orderDocument, _hostEnvironment);

                    if (orderDocumentValidation != null || orderDocumentValidation?.Status == Status.BusinessException || orderDocumentValidation?.Status == Status.Error)
                    {
                        analyticsEventCommand.Track("ServiceName", "OrderDocumentValidation"); 
                        analyticsEventCommand.Track($"ErrorId", orderDocumentValidation?.ResponseMessages?.Errors?.First().Id);
                        return ErrorDetails(orderDocumentValidation.ResponseMessages, analyticsEventCommand);
                    }

                }

                var error = _requestValidations.ValidateServiceListOrder(orderDocument, isOfferServiceRq, offerDocument);

                if (error?.Status == Status.BusinessException || error?.Status == Status.Error)
                {
                    analyticsEventCommand.Track("ServiceName", "ValidateServiceListOrder");
                    analyticsEventCommand.Track($"ErrorId", error?.ResponseMessages?.Errors?.First().Id);
                    return ErrorDetails(error.ResponseMessages, analyticsEventCommand);
                }

                //Validation API 
                var corporateAccount = (offerDocument != null && offerDocument.Offers.Any()) ? new CorporateAccount()
                {
                    AgencyID = offerDocument.Parties.CorporateAccount?.AgencyID,
                    AccountName = offerDocument.Parties.CorporateAccount?.AccountName,
                    CorporateCode = offerDocument.Parties.CorporateAccount?.CorporateCode
                } : orderDocument?.Parties?.CorporateAccount;

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(ValidationMSRequestMapper.GetServiceListParties(request.ServiceListRQ.DistributionChain, corporateAccount));
                var validationMSRS = await _mediator.Send(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest, AgentSubscriptionKey = request.SubscriptionKey }, cancellationToken);

                if (validationMSRS?.Error != null)
                {
                    var validationMSErrors = _requestValidations.ValidateAgency(validationMSRS, orderDocument, isOfferServiceRq, offerDocument);

                    if (validationMSErrors != null)
                    {
                        analyticsEventCommand.Track("ServiceName", "ValidationMS");
                        analyticsEventCommand.Track("ErrorCode", validationMSErrors.ResponseMessages.Errors.First().Code);
                        analyticsEventCommand.Track("ErrorStatusCode", StatusCodes.Status422UnprocessableEntity);

                        return new Response<IATA_ServiceListRS>()
                        {
                            Status = Status.Error,
                            Value = MapToErrorServiceListRs(validationMSErrors.ResponseMessages)
                        };
                    }
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
                }

                if (offerDocument != null && isOfferServiceRq)
                {
                    analyticsEventCommand.Track("PosCountry", offerDocument.Pos?.CountryCode);
                    analyticsEventCommand.Track("PosCity", offerDocument.Pos?.CityCode);

                    bool oneWay = offerDocument.DataList?.OriginDestinationAssociations?.Count() == 1;
                    bool roundtrip = false;

                    if (offerDocument.DataList?.OriginDestinationAssociations?.Count() == 2)
                    {
                        var outboundOrigin = offerDocument.DataList.OriginDestinationAssociations?.First().OriginDestination.OriginAirport;
                        var outboundDestination = offerDocument.DataList.OriginDestinationAssociations?.First().OriginDestination.DestinationAirport;
                        var inboundOrigin = offerDocument.DataList.OriginDestinationAssociations?.First().OriginDestination.OriginAirport;
                        var inboundDestination = offerDocument.DataList.OriginDestinationAssociations?.First().OriginDestination.DestinationAirport;

                        if (outboundOrigin == inboundDestination && outboundDestination == inboundOrigin)
                        {
                            roundtrip = true;
                        }
                    }
                    analyticsEventCommand.Track("Oneway", oneWay ? 'Y' : 'N');
                    analyticsEventCommand.Track("Roundtrip", roundtrip ? 'Y' : 'N');
                    analyticsEventCommand.Track("Multicity", (!oneWay && !roundtrip) ? 'Y' : 'N');

                    for (int i = 1; i <= offerDocument.DataList?.OriginDestinationAssociations?.Count(); i++)
                    {
                        var ond = offerDocument.DataList?.OriginDestinationAssociations.ElementAt(i - 1);
                        analyticsEventCommand.Track($"OriginDestination{i}", ond.OriginDestination.OriginAirport + ond.OriginDestination.DestinationAirport);
                        analyticsEventCommand.Track($"DepartureDate{i}", ond.OriginDestination.DepartureDate);
                    }

                    var paxGroup = offerDocument.DataList?.CustomerDetails?.GroupBy(x => x.Ptc).Select(x => new { Pax = x.Key, PaxCount = x.Count() });
                    for (int i = 1; i <= paxGroup?.Count(); i++)
                    {
                        analyticsEventCommand.Track($"PaxType{i}", paxGroup.ElementAt(i - 1).Pax);
                        analyticsEventCommand.Track($"PaxCount{i}", paxGroup.ElementAt(i - 1).PaxCount);
                    }
                }

                var getReservationOrderDoc = new OrderDocument();

                if (orderDocument != null)
                {
                    var getReservationReq = OrderSearchMSRequestMapper.MapOrderSearchRequest(orderDocument);

                    getReservationOrderDoc = await _mediator.Send(new OrderSearchMSCommand(getReservationReq), cancellationToken);

                    if (getReservationOrderDoc?.DataList != null)
                    {
                        analyticsEventCommand.Track("PosCountry", getReservationOrderDoc.PointOfSale?.CountryCode);
                        analyticsEventCommand.Track("PosCity", getReservationOrderDoc.PointOfSale?.CityCode);
                        analyticsEventCommand.Track("Rloc", getReservationOrderDoc.BookingReference.RLOC);

                        bool oneWay = getReservationOrderDoc.DataList.OriginDestinationAssociations?.Count() == 1;
                        bool roundtrip = false;

                        if (getReservationOrderDoc.DataList.OriginDestinationAssociations?.Count() == 2)
                        {
                            var outboundOrigin = getReservationOrderDoc.DataList.OriginDestinationAssociations?.First().OriginDestination.OriginAirport;
                            var outboundDestination = getReservationOrderDoc.DataList.OriginDestinationAssociations?.First().OriginDestination.DestinationAirport;
                            var inboundOrigin = getReservationOrderDoc.DataList.OriginDestinationAssociations?.First().OriginDestination.OriginAirport;
                            var inboundDestination = getReservationOrderDoc.DataList.OriginDestinationAssociations?.First().OriginDestination.DestinationAirport;

                            if (outboundOrigin == inboundDestination && outboundDestination == inboundOrigin)
                            {
                                roundtrip = true;
                            }
                        }

                        analyticsEventCommand.Track("Oneway", oneWay ? 'Y' : 'N');
                        analyticsEventCommand.Track("Roundtrip", roundtrip ? 'Y' : 'N');
                        analyticsEventCommand.Track("Multicity", (!oneWay && !roundtrip) ? 'Y' : 'N');

                        for (int i = 1; i <= getReservationOrderDoc.DataList.OriginDestinationAssociations?.Count(); i++)
                        {
                            var ond = getReservationOrderDoc.DataList.OriginDestinationAssociations.ElementAt(i - 1);
                            analyticsEventCommand.Track($"OriginDestination{i}", ond.OriginDestination.OriginAirport + ond.OriginDestination.DestinationAirport);
                            analyticsEventCommand.Track($"DepartureDate{i}", ond.OriginDestination.DepartureDate);
                        }

                        var pax = getReservationOrderDoc.DataList.CustomerDetails.Select(customer => new { Pax = customer.Ptc == Constants.Application.CNNPTCCode ? Constants.Application.ChildPTCCode : customer.Ptc });
                        var paxGroup = pax.GroupBy(x => x.Pax).Select(x => new { Pax = x.Key, PaxCount = x.Count() });

                        for (int i = 1; i <= paxGroup.Count(); i++)
                        {
                            analyticsEventCommand.Track($"PaxType{i}", paxGroup.ElementAt(i - 1).Pax);
                            analyticsEventCommand.Track($"PaxCount{i}", paxGroup.ElementAt(i - 1).PaxCount);
                        }
                    }

                    if (getReservationOrderDoc is not null)
                    {
                        if (orderDocument.DataList.CustomerDetails?.FirstOrDefault()?.Contacts?.CountryDailingCode != null)
                        {
                            getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = orderDocument.DataList.CustomerDetails.First().Contacts.CountryDailingCode;
                        }
                        foreach (var customer in orderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
                        {
                            getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
                        }


                        foreach (var customer in orderDocument.DataList.CustomerDetails.Where(a => a.IsApisAdded))
                        {
                            getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsApisAdded = true;
                        }

                        if (orderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderDocument.FareTypeRequest;

                        foreach (var customer in orderDocument.DataList.CustomerDetails)
                        {
                            var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                            if (getReservationOrderDocCustomer != null)
                            {
                                getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                            }
                        }

                        getReservationOrderDoc = OrderDataMsToSaberDocumentMapper.RetainPriceDetails(orderDocument, getReservationOrderDoc);
                        IEnumerable<Infrastructure.Model.BrandMS.BrandRS> brandResponse;

                        // calling BrandMS
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

                        getReservationOrderDoc.ChangeType =orderDocument.ChangeType ?? getReservationOrderDoc.ChangeType;
                        getReservationOrderDoc.ChangeDateTime = orderDocument.ChangeDateTime ?? getReservationOrderDoc.ChangeDateTime;
                        getReservationOrderDoc.Version = orderDocument.Version;
                        getReservationOrderDoc.BookingReference = orderDocument.BookingReference;

                        getReservationOrderDoc = BrandMsRSMapper.MapBrandMSResponseUsingSabreOrderDoc(getReservationOrderDoc, brandResponse, orderDocument);

                        var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };
                        var publishOrderDocResponse = await _mediator.Send(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);
                    }
                }

                var warningList = getReservationOrderDoc?.DataList != null ? _responseValidations.PopulateNonVSSegmentWarningsOrder(getReservationOrderDoc) : null;

                var warningOfferList = offerDocument?.Offers?.Any() ?? false ? _responseValidations.PopulateNonVSSegmentWarningsOffer(offerDocument) : null;

                var flightServiceRequestcommand = FligthServiceRQmapper.FlightServiceRequestMapper(request, getReservationOrderDoc, offerDocument);

                var response = await _mediator.Send(flightServiceRequestcommand, cancellationToken);

                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);

                return ServiceListRSMapper.MapServiceListResponse(offerDocument, isOfferServiceRq, getReservationOrderDoc, _genericDataMapper, warningList, warningOfferList, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Service List Query Handler");

                analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);

                return new Response<IATA_ServiceListRS>()
                {
                    Status = Status.BusinessException,
                    Value = MapToErrorServiceListRs(DefaultError.DefaultErrorMesaage())
                };
            }
            finally
            {
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        private Response<IATA_ServiceListRS> ErrorDetails(ResponseMessage responseMessage, TrackAnalyticsEventCommand analyticsEventCommand)
        {
            var resultCode = ErrorCodeMapper.MapErrorResultCode(responseMessage);

            string statusCode = resultCode == Status.BusinessException ? Constants.Application.BusinessException : resultCode == Status.Error ? Constants.Application.Error : resultCode == Status.Unauthorized ? Constants.Application.Unauthorized : string.Empty; ;
            
            analyticsEventCommand.Track("StatusCode", statusCode);
            analyticsEventCommand.Track("ErrorCode", responseMessage.Errors.First().Code);

            DateTime? dateOfResponse = DateTime.UtcNow;
            analyticsEventCommand.Track("DateOfResponse", dateOfResponse);

            return new Response<IATA_ServiceListRS>()
            {
                Status = resultCode,
                Value = MapToErrorServiceListRs(responseMessage)
            };
        }


        private IATA_ServiceListRS MapToErrorServiceListRs(ResponseMessage errors)
        {
            return new IATA_ServiceListRS
            {
                Items = errors.Errors.Select(error =>
                        new ErrorType()
                        {
                            Code = error.Code,
                            DescText = error.Message,
                            LangCode = Constants.Application.LangCode,
                            TypeCode = error.StatusText,
                            TagText = error.TagText
                        }).ToArray()
            };

        }
    }

}
