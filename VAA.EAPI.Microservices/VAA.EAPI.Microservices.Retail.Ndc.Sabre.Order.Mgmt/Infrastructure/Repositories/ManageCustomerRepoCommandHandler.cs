using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories
{
    public class ManageCustomerRepoCommandHandler : IRequestHandler<ManageCustomerRepoCommand, Result<Response<OrderDocument>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManageCustomerRepoCommandHandler> _logger;
        private readonly IUpdateReservationCommand _updateReservationCommand;
        private readonly GetReservationQueryHandler _getReservationQueryHandler;
        private readonly EnhancedEndTransactionCommandHandler _enhancedEndTransactionCommandHandler;
        private readonly SabreCommandLLSCommandHandler _sabreCommandLLSCommandHandler;
        private readonly EnhancedAirTicketCommandHandler _enhancedAirTicketCommandHandler;
        private readonly GetElectronicDocumentQueryHandler _getElectronicDocumentQueryHandler;
        private readonly SessionCloseCommandHandler _sessionCloseCommandHandler;
        public ManageCustomerRepoCommandHandler(IMediator mediator, ILogger<ManageCustomerRepoCommandHandler> logger, IUpdateReservationCommand updateReservationCommand, GetReservationQueryHandler getReservationQueryHandler, EnhancedEndTransactionCommandHandler enhancedEndTransactionCommandHandler, SabreCommandLLSCommandHandler sabreCommandLLSCommandHandler, EnhancedAirTicketCommandHandler enhancedAirTicketCommandHandler, GetElectronicDocumentQueryHandler getElectronicDocumentQueryHandler, SessionCloseCommandHandler sessionCloseCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _updateReservationCommand = updateReservationCommand ?? throw new ArgumentNullException(nameof(updateReservationCommand));
            _getReservationQueryHandler = getReservationQueryHandler ?? throw new ArgumentNullException(nameof(getReservationQueryHandler));
            _enhancedEndTransactionCommandHandler = enhancedEndTransactionCommandHandler ?? throw new ArgumentNullException(nameof(enhancedEndTransactionCommandHandler));
            _sabreCommandLLSCommandHandler = sabreCommandLLSCommandHandler ?? throw new ArgumentNullException(nameof(sabreCommandLLSCommandHandler));
            _enhancedAirTicketCommandHandler = enhancedAirTicketCommandHandler ?? throw new ArgumentNullException(nameof(enhancedAirTicketCommandHandler));
            _getElectronicDocumentQueryHandler = getElectronicDocumentQueryHandler ?? throw new ArgumentNullException(nameof(getElectronicDocumentQueryHandler));
            _sessionCloseCommandHandler = sessionCloseCommandHandler ?? throw new ArgumentNullException(nameof(sessionCloseCommandHandler));
        }

        public async Task<Result<Response<OrderDocument>>> Handle(ManageCustomerRepoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _mediator.Send(new CreateSessionCommand() { PCC = request.ManageCustomerRequest.Parties.Participants.First().PccCode }, cancellationToken);

                if (token != null)
                {
                    var getReservationRQ = GetReservationRequestMapper.GetReservationRequest(token, request.ManageCustomerRequest.Rloc);
                    var getReservationRS = await _mediator.Send(new GetReservationQuery(getReservationRQ), cancellationToken);

                    if (getReservationRS != null || getReservationRS?.Errors == null)
                    {
                        var passengers = (getReservationRS.Item as EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.ReservationPNRB).PassengerReservation.Passengers.Passenger;
                        var customerDetails = request.ManageCustomerRequest.CustomerDetails.First();
                        bool isAmendApis = request.ManageCustomerRequest.AmendApis;
                        List<TravelDocument> filteredTravelDocs = customerDetails.TravelDocuments;

                        if (isAmendApis)
                        {
                            var paxName = passengers.First(x => x.nameId.Split(".").First() == customerDetails.LastNameNumber);
                            var associatedAdult = customerDetails.Ptc == Constants.Application.Infant ? passengers.FirstOrDefault(x => x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[2] == paxName.FirstName && x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[1] == paxName.LastName) : null;
                            var infantApisDocs = associatedAdult == null ? null : associatedAdult.SpecialRequests.APISRequest.Where(x => x.DOCSEntry != null && x.DOCSEntry.Forename == paxName.FirstName && x.DOCSEntry.Surname == paxName.LastName || x.DOCOEntry != null && x.DOCOEntry.FreeText.Contains("/I"));
                            var specialReqs = customerDetails.Ptc == Constants.Application.Infant ? infantApisDocs : passengers.First(x => x.passengerType == (customerDetails.Ptc == "CHD" ? "CNN" : customerDetails.Ptc == "GBE" ? "YTH" : customerDetails.Ptc) && x.nameId.Split(".").First() == customerDetails.LastNameNumber).SpecialRequests.APISRequest;

                            var unsuccessfulAPIS = customerDetails.TravelDocuments.Where(doc => ((doc.DocumentType == "PT" || doc.DocumentType == "GC") && !specialReqs.Any(a => a.DOCSEntry != null && a.DOCSEntry.DocumentNumber == doc.DocumentNumber && a.DOCSEntry.Forename == paxName.FirstName && a.DOCSEntry.Surname == paxName.LastName))
                                                                                         || (doc.DocumentType == "CR" && !specialReqs.Any(a => a.DOCOEntry != null && a.DOCOEntry.FreeText.Contains("/K/") && a.DOCOEntry.FreeText.Contains($"/{doc.DocumentNumber}/")))
                                                                                         || (doc.DocumentType == "AEA" && !specialReqs.Any(a => a.DOCOEntry != null && a.DOCOEntry.FreeText.Contains("/R/") && a.DOCOEntry.FreeText.Contains($"/{doc.DocumentNumber}/")))
                                                                                         || (doc.DocumentType == "VI" && !specialReqs.Any(a => a.DOCOEntry != null && a.DOCOEntry.FreeText.Contains("/V/") && a.DOCOEntry.FreeText.Contains($"/{doc.DocumentNumber}/")))).Select(doc => doc.DocumentType);
                                
                            filteredTravelDocs = customerDetails.TravelDocuments.Where(doc => !unsuccessfulAPIS.Contains(doc.DocumentType)).ToList();
                        }

                        if ((isAmendApis && filteredTravelDocs.Any()) || request.ManageCustomerRequest.TicketedNameCorrection || request.ManageCustomerRequest.UnTicketedNameCorrection)
                        {
                            var updateReservationRQ = UpdateReservationRequestMapper.GetUpdateReservationRequest(token, request.ManageCustomerRequest, passengers);
                            var updateReservationRS = await _updateReservationCommand.Handle(updateReservationRQ, cancellationToken);

                            if (updateReservationRS.UpdateReservationRS?.Errors != null)
                            {
                                _logger.LogError("Sabre UpdateReservation returned an error response");
                                 return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                            }
                        }
                        var updatePassengerNameRecordRQRequest = ManageCustomerRequestMapper.CustomerPnrReqMapper(request.ManageCustomerRequest, passengers, token);
                        var manageCustomerPnrResponse = await _mediator.Send(new UpdatePassengerNameRecordCommand() { UpdatePassengerNameRecordRQRequest = updatePassengerNameRecordRQRequest }, cancellationToken);

                        if (manageCustomerPnrResponse.UpdatePassengerNameRecordRS.ApplicationResults?.Error != null)
                        {
                            _logger.LogError("Sabre UpdatePassengerNameRecord returned an error response");
                            if(manageCustomerPnrResponse.UpdatePassengerNameRecordRS.ApplicationResults?.Warning != null)
                            {
                                var warningMessages = manageCustomerPnrResponse.UpdatePassengerNameRecordRS.ApplicationResults.Warning.SelectMany(x => x.SystemSpecificResults).SelectMany(x => x.Message).Select(x => x.Value);
                                if(warningMessages.Any(x => x.Contains("MEAL ORDER EXISTS FOR THIS CUSTOMER")))
                                {
                                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand() { BinaryToken = token, PCC = request.ManageCustomerRequest.Parties.Participants.First().PccCode }, cancellationToken);
                                    return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.SSROrderExistsForCustomer, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                                }

                            }
                            return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                        }

                        if(request.ManageCustomerRequest.TicketedNameCorrection)
                        {
                            getReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRQ), cancellationToken);
                            var nameid = $"{request.ManageCustomerRequest.CustomerDetails.First().LastNameNumber}.{request.ManageCustomerRequest.CustomerDetails.First().FirstNameNumber}";
                            var pax = (getReservationRS.Item as EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.ReservationPNRB).PassengerReservation;
                            var passgener = pax.Passengers.Passenger;
                            var ticketInfo = pax.TicketingInfo.TicketDetails.Where(t => t.PassengerName?.nameId == nameid && t.Status?.ToUpper() == "ACTIVE").First().index;
                            var passengerInfo = passgener.First(x => x.passengerType == (customerDetails.Ptc == "CHD" ? "CNN" : customerDetails.Ptc == "GBE" ? "YTH" : customerDetails.Ptc) && x.nameId.Split(".").First() == customerDetails.LastNameNumber);
                            var pqValue = (getReservationRS.PriceQuote.Item as VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.PriceQuote.PriceQuoteInfoGetResponse).Details.Where(x =>x.passengerType == (customerDetails.Ptc == "CHD" ? "CNN" : customerDetails.Ptc == "GBE" ? "YTH" : customerDetails.Ptc)).OrderByDescending(x =>x.TransactionInfo.CreateDateTime).First().number;

                            var hostCommands = new string[]
                            {
                                $"WFRTR{ticketInfo}¥AVS¥PQ{pqValue}¥N{passengerInfo.nameId}",
                                string.Empty,
                                string.Empty,
                               "<![CDATA[ WF<0><><><><><X><><> ]]>",
                            };

                            foreach (var hostCommand in hostCommands)
                            {
                                var sabreCommandLLSRQ = SabreCommandLLSRequestMapper.GetSabreCommandRequest(token, hostCommand);
                                var sabreCommandLLSRS = await _sabreCommandLLSCommandHandler.Handle(new SabreCommandLLSCommand() { SabreCommandLLSRequest = sabreCommandLLSRQ }, cancellationToken);
                                if (sabreCommandLLSRS != null || sabreCommandLLSRS.ApplicationResults?.Error != null)
                                {
                                    if (hostCommand.StartsWith("WFR"))
                                    {
                                        var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                        var matchCount = sabreCommandResponse1.Sum(x => new Regex(Regex.Escape("< >")).Matches(x).Count);
                                        var command1 = new StringBuilder();
                                        for (int j = 0; j < matchCount; j++)
                                        {
                                            command1.Append(j < matchCount - 2 ? "<X>" : "< >");
                                        }
                                        hostCommands[1] = $"<![CDATA[ WF{command1}]]>";
                                    }
                                    else if (hostCommand.StartsWith("<![CDATA[ WF<X><X>"))
                                    {
                                        var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                        var matchCount = sabreCommandResponse1.Sum(x => new Regex(Regex.Escape("< >")).Matches(x).Count);
                                        var totalFields = sabreCommandResponse1.Sum(line => Regex.Matches(line, @"<\s*>").Count);

                                        var command2 = new StringBuilder();

                                        for (int i = 0; i < totalFields; i++)
                                        {
                                            command2.Append(i == totalFields - 2 ? "<X>" : "<>");
                                        }

                                        hostCommands[2] = $"<![CDATA[ WF{command2} ]]>";
                                    }                                   
                                    
                                }
                                if (sabreCommandLLSRS.ApplicationResults?.Error != null)
                                {
                                    _logger.LogError("SabreCommandLLS returned an error response");
                                }
                            }

                            var enhancedEndTransactionRS = await _enhancedEndTransactionCommandHandler.Handle(new EnhancedEndTransactionCommand() { EnhancedEndTransactionRQ = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token) }, cancellationToken);
                        }

                        getReservationRS = await _mediator.Send(new GetReservationQuery(getReservationRQ), cancellationToken);

                        AirTicketRS airTicketRS = null;
                        if (request.ManageCustomerRequest.TicketedNameCorrection)
                        {
                            var pricedItinerary = ((VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.ReservationPNRB)getReservationRS.Item).PassengerReservation.ItineraryPricing.PricedItinerary;

                            var airTicketRQ = EnhancedAirTicketRequestMapper.GetEnhancedAirTicketRequest(token, request.ManageCustomerRequest.Rloc, pricedItinerary, request.ManageCustomerRequest.Parties.Participants.First().PccCode);
                            airTicketRS = await _enhancedAirTicketCommandHandler.Handle(new EnhancedAirTicketCommand() { EnhancedAirTicketRequest = airTicketRQ }, cancellationToken);
                            
                            if (airTicketRS.ApplicationResults?.Error != null)
                            {
                                _logger.LogError("Sabre AirTicket returned an error response");
                            }
                        }

                        var response = ManageCustomerResponseMapper.SabreToManageCustomerResponseMapper(getReservationRS, request.ManageCustomerRequest, airTicketRS);

                        if (request.ManageCustomerRequest.TicketedNameCorrection)
                        {
                            foreach (var ticket in response.OrderItems.SelectMany(x => x.AccountableDocumentRefs).DistinctBy(x => x.DocumentNumber))
                            {
                                var couponResponse = await _getElectronicDocumentQueryHandler.Handle(new GetElectronicDocumentQuery() { GetElectronicDocumentRQRequest = GetElectronicDocumentRequestMapper.GetElectronicDocumentMapper(token, ticket) }, cancellationToken);
                                if (couponResponse?.DocumentDetailsDisplay != null)
                                {
                                    response.OrderItems = response.OrderItems.Select(orderItem =>
                                    {
                                        var accountableDocumentRefsList = orderItem.AccountableDocumentRefs.ToList();
                                        for (int i = 0; i < accountableDocumentRefsList.Count; i++)
                                        {
                                            if (accountableDocumentRefsList[i].DocumentNumber == ticket.DocumentNumber && ((TicketingDocumentTicketED)couponResponse.DocumentDetailsDisplay.Item).RelatedDocument != null)
                                            {
                                                accountableDocumentRefsList[i] = IssueTicketDataMapper.AccountableDocumentsMapper(ticket, couponResponse, response);
                                                break;
                                            }
                                        }
                                        orderItem.AccountableDocumentRefs = accountableDocumentRefsList;
                                        return orderItem;
                                    });
                                }

                            }
                        }

                        await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = request.ManageCustomerRequest.Parties.Participants.First().PccCode }, cancellationToken);

                        return Result.Success(new Response<OrderDocument>
                        {
                            Status = Status.OK,
                            Value = response
                        });
                    }
                    
                    else
                    {
                        _logger.LogError("Sabre GetReservation returned an error response");
                    }
                }

                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Manage customer Sabre workflow failed");
                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
