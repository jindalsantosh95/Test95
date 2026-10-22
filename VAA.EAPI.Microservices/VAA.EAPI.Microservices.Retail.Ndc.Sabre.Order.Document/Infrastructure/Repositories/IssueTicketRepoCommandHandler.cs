using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.ManualPriceQuoteAddInfoLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure.Repositories
{
    public class IssueTicketRepoCommandHandler : IRequestHandler<IssueTicketRepoCommand, Result<Response<IssueTicketResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IssueTicketRepoCommandHandler> _logger;

        public IssueTicketRepoCommandHandler(IMediator mediator, ILogger<IssueTicketRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IssueTicketResponse>>> Handle(IssueTicketRepoCommand request, CancellationToken cancellationToken)
        {
            string pccCode = null;
            string token = null;
            var sessionCloseAttempted = false;

            try
            {
                pccCode = request.IssueTicketRequest.parties.Participants.First().PccCode;

                token = await _mediator.Send(new CreateSessionCommand() { PCC = pccCode }, cancellationToken);

                if (token != null)
                {
                    var getReservationRequest = GetReservationRQMapper.GetReservationRequest(token, request.IssueTicketRequest.BookingReferences.RLOC);
                    var getReservationresponse = await _mediator.Send(new GetReservationQuery(getReservationRequest), cancellationToken);

                    ManualPriceQuoteAddInfoRS manualPriceQuoteContentRS = new ManualPriceQuoteAddInfoRS();
                    ManualPriceQuoteAddInfoRQRequest manualPriceQuoteContentRQ = new ManualPriceQuoteAddInfoRQRequest();

                    if (getReservationresponse != null && getReservationresponse.Errors == null)
                    {
                        bool invalidAir4Pnr = ((getReservationresponse.Item as ReservationPNRB).PassengerReservation.Segments.Segment.Any(x =>
                        {
                            var airSeg = x.Item as SegmentTypePNRBSegmentAir;
                            return airSeg?.AirlineRefId?.Equals("DCVS*", StringComparison.OrdinalIgnoreCase) == true;
                        }));

                        if (invalidAir4Pnr)
                        {
                            _logger.LogWarning("Sabre operation returned an invalid AirlineRefId. {Operation} {PnrLocator}", "GetReservation", request.IssueTicketRequest.BookingReferences.RLOC);
                        }

                        var deltaPNR = ((getReservationresponse.Item as ReservationPNRB).PassengerReservation.Segments.Segment.First().Item as SegmentTypePNRBSegmentAir).AirlineRefId[^6..];
                        var passengers = (getReservationresponse.Item as ReservationPNRB).PassengerReservation.Passengers.Passenger;
                        var flightSegments = (getReservationresponse.Item as ReservationPNRB).PassengerReservation.Segments.Segment;                        
                        bool actionCode = flightSegments.Any(x => (x.Item as SegmentTypePNRBSegmentAir)?.ActionCode == "TK") ;
                        
                        foreach (var ptcType in request.IssueTicketRequest.CustomerDetails.GroupBy(x => x.Ptc).Select((x, i) => new { Value = x.Key, Index = i }))
                        {
                            var manualPriceQuoteCreateRQ = ManualPriceQuoteRQMapper.ManualPriceQuoteAddInfoRQRequest(token, false, request.IssueTicketRequest, ptcType.Value, ptcType.Index, passengers);
                            var manualPriceQuoteCreateRS = await _mediator.Send(new ManualPriceQuoteAddInfoCommand() { ManualPriceQuoteRequest = manualPriceQuoteCreateRQ }, cancellationToken);
                            
                            if (manualPriceQuoteCreateRS != null && manualPriceQuoteCreateRS.ApplicationResults?.Error == null)
                            {
                                manualPriceQuoteContentRQ = ManualPriceQuoteRQMapper.ManualPriceQuoteAddInfoRQRequest(token, true, request.IssueTicketRequest, ptcType.Value, ptcType.Index, passengers);
                                manualPriceQuoteContentRS = await _mediator.Send(new ManualPriceQuoteAddInfoCommand() { ManualPriceQuoteRequest = manualPriceQuoteContentRQ }, cancellationToken);
                            }
                            else
                            {
                                _logger.LogError("Sabre operation returned errors. {Operation} {PnrLocator}", "ManualPriceQuoteAddInfo", request.IssueTicketRequest.BookingReferences.RLOC);

                                // close sabre session
                                if (!string.IsNullOrWhiteSpace(token))
                                {
                                    await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = pccCode }, cancellationToken);
                                    sessionCloseAttempted = true;
                                }

                                throw new Exception("Failure in ManualPriceQuoteCreateResponse call");
                            }

                        }

                        if (manualPriceQuoteContentRS != null && manualPriceQuoteContentRS.ApplicationResults?.Error == null)
                        {
                            var enhancedEndTransactionRQ = EnhancedEndTransactionRQMapper.EnhancedEndTransactionRQ(token, actionCode);
                            var response = await _mediator.Send(new EnhancedEndTransactionCommand() { EnhancedEndTransactionRQ = enhancedEndTransactionRQ }, cancellationToken);

                            if (response != null)
                            {
                                var enhancedAirTicketRequest = IssueTicketDataMapper.GetAirticketRequest(token, request.IssueTicketRequest, pccCode);

                                var airTicketResponse = await _mediator.Send(new EnhancedAirTicketCommand() { EnhancedAirTicketRequest = enhancedAirTicketRequest }, cancellationToken);
                                var issueTicketRespone = new IssueTicketResponse() { BookingReference = new() { RLOC = request.IssueTicketRequest.BookingReferences.RLOC, ExternalRecordLocator = deltaPNR }, SegmentDetails = request.IssueTicketRequest.SegmentDetails };

                                foreach (var seg in issueTicketRespone.SegmentDetails)
                                {
                                    var airSegment = flightSegments.FirstOrDefault(x => (x.Item as SegmentTypePNRBSegmentAir)?.DepartureAirport == seg.OriginAirportCode && (x.Item as SegmentTypePNRBSegmentAir)?.ArrivalAirport == seg.DestinationAirportCode).Item as SegmentTypePNRBSegmentAir;
                                    seg.Status.Code = airSegment.ActionCode;
                                }

                                if (airTicketResponse?.Summary != null)
                                {
                                    var accountableDocRefSequences = new List<IEnumerable<AccountableDocumentRef>>();
                                    var accountableDocumentRefInitialized = false;

                                    foreach (var ticket in airTicketResponse.Summary)
                                    {
                                        var couponRespose = await _mediator.Send(new GetElectronicDocumentQuery() { GetElectronicDocumentRQRequest = GetElectronicDocumentRQMapper.GetElectronicDocumentRQRequest(token, ticket.DocumentNumber, ticket.DocumentType) }, cancellationToken);
                                        if (couponRespose?.DocumentDetailsDisplay != null)
                                        {
                                            var accountableDocRef = IssueTicketDataMapper.AccountableDocumentsMapper(ticket, couponRespose, request.IssueTicketRequest.CustomerDetails, request.IssueTicketRequest.SegmentDetails);
                                            if (accountableDocRef == null)
                                            {
                                                continue;
                                            }

                                            accountableDocRefSequences.Add(accountableDocRef);
                                            accountableDocumentRefInitialized = true;
                                        }

                                    }

                                    if (accountableDocumentRefInitialized)
                                    {
                                        issueTicketRespone.AccountableDocumentRef = accountableDocRefSequences.SelectMany(x => x);
                                    }
                                }
                                else
                                {
                                    _logger.LogError("Sabre operation returned no ticket summary. {Operation} {PnrLocator}", "EnhancedAirTicket", request.IssueTicketRequest.BookingReferences.RLOC);

                                    // close sabre session
                                    if (!string.IsNullOrWhiteSpace(token))
                                    {
                                        await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = pccCode }, cancellationToken);
                                        sessionCloseAttempted = true;
                                    }

                                    throw new Exception("Failure in AirTicket call");
                                }

                                // close sabre session
                                if (!string.IsNullOrWhiteSpace(token))
                                {
                                    await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = pccCode }, cancellationToken);
                                    sessionCloseAttempted = true;
                                }

                                IssueTicketDataMapper.MapSSROrderItems(getReservationresponse, request.IssueTicketRequest, issueTicketRespone);

                                return Result.Success(new Response<IssueTicketResponse>
                                {
                                    Status = Status.OK,
                                    Value = issueTicketRespone
                                });
                            }
                            
                        }

                        else
                        {
                            _logger.LogError("Sabre operation returned errors. {Operation} {PnrLocator}", "ManualPriceQuoteAddInfo", request.IssueTicketRequest.BookingReferences.RLOC);

                            // close sabre session
                            if (!string.IsNullOrWhiteSpace(token))
                            {
                                await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = pccCode }, cancellationToken);
                                sessionCloseAttempted = true;
                            }

                            throw new Exception("Failure in ManualPriceQuoteContentResponse call");
                        }

                    }

                    else
                    {
                        _logger.LogError("Sabre operation returned no reservation. {Operation} {PnrLocator}", "GetReservation", request.IssueTicketRequest.BookingReferences.RLOC);
                    }
                }

                // close sabre session
                if (!sessionCloseAttempted && !string.IsNullOrWhiteSpace(token))
                {
                    await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = pccCode }, cancellationToken);
                    sessionCloseAttempted = true;
                }

                return Result.Failure<Response<IssueTicketResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (ExternalServiceIntegrationException)
            {
                _logger.LogError("IssueTicket processing failed. {Operation} {PnrLocator}", "IssueTicket", request?.IssueTicketRequest?.BookingReferences?.RLOC);
                return Result.Failure<Response<IssueTicketResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IssueTicket processing failed. {Operation} {PnrLocator}", "IssueTicket", request?.IssueTicketRequest?.BookingReferences?.RLOC);
                return Result.Failure<Response<IssueTicketResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                if (!sessionCloseAttempted && !string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(pccCode))
                {
                    try
                    {
                        await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = pccCode }, cancellationToken);
                        sessionCloseAttempted = true;
                    }
                    catch (Exception)
                    {
                        _logger.LogWarning("Sabre session close failed. {Operation} {PnrLocator}", "SessionClose", request?.IssueTicketRequest?.BookingReferences?.RLOC);
                    }
                }
            }
        }
    }
}
