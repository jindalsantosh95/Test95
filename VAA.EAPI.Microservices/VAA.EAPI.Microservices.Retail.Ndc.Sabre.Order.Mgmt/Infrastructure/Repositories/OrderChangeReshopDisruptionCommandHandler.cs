using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories
{
    public class OrderChangeReshopDisruptionCommandHandler : IOrderChangeReshopDisruptionCommand
    {
        private readonly CreateSessionCommandHandler _createSessionCommandHandler;
        private readonly GetReservationQueryHandler _getReservationQueryHandler;
        private readonly EnhancedEndTransactionCommandHandler _enhancedEndTransactionCommandHandler;
        private readonly OTACancelLLSCommandHandler _otaCancelLLSCommandHandler;
        private readonly EnhancedAirBookCommandHandler _enhancedAirBookCommandHandler;
        private readonly DesignatePrinterCommandHandler _designatePrinterCommandHandler;
        private readonly ITKTExchangeCommand _tktExchangeCommand;
        private readonly GetElectronicDocumentQueryHandler _getElectronicDocumentQueryHandler;
        private readonly SessionCloseCommandHandler _sessionCloseCommandHandler;
        private readonly ILogger<OrderChangeReshopDisruptionCommandHandler> _logger;
        private readonly UpdatePassengerNameRecordCommandHandler _updatePassengerNameRecordCommandHandler;

        public OrderChangeReshopDisruptionCommandHandler(CreateSessionCommandHandler createSessionCommandHandler, GetReservationQueryHandler getReservationQueryHandler, EnhancedEndTransactionCommandHandler enhancedEndTransactionCommandHandler, OTACancelLLSCommandHandler otaCancelLLSCommandHandler, EnhancedAirBookCommandHandler enhancedAirBookCommandHandler, DesignatePrinterCommandHandler designatePrinterCommandHandler, ITKTExchangeCommand tktExchangeCommand, GetElectronicDocumentQueryHandler getElectronicDocumentQueryHandler, SessionCloseCommandHandler sessionCloseCommandHandler, ILogger<OrderChangeReshopDisruptionCommandHandler> logger, UpdatePassengerNameRecordCommandHandler updatePassengerNameRecordCommandHandler)
        {
            _createSessionCommandHandler = createSessionCommandHandler ?? throw new ArgumentNullException(nameof(createSessionCommandHandler));
            _getReservationQueryHandler = getReservationQueryHandler ?? throw new ArgumentNullException(nameof(getReservationQueryHandler));
            _enhancedEndTransactionCommandHandler = enhancedEndTransactionCommandHandler ?? throw new ArgumentNullException(nameof(enhancedEndTransactionCommandHandler));
            _otaCancelLLSCommandHandler = otaCancelLLSCommandHandler ?? throw new ArgumentNullException(nameof(otaCancelLLSCommandHandler));
            _enhancedAirBookCommandHandler = enhancedAirBookCommandHandler ?? throw new ArgumentNullException(nameof(enhancedAirBookCommandHandler));
            _designatePrinterCommandHandler = designatePrinterCommandHandler ?? throw new ArgumentNullException(nameof(designatePrinterCommandHandler));
            _tktExchangeCommand = tktExchangeCommand ?? throw new ArgumentNullException(nameof(tktExchangeCommand));
            _getElectronicDocumentQueryHandler = getElectronicDocumentQueryHandler ?? throw new ArgumentNullException(nameof(getElectronicDocumentQueryHandler));
            _sessionCloseCommandHandler = sessionCloseCommandHandler ?? throw new ArgumentNullException(nameof(sessionCloseCommandHandler));
            _updatePassengerNameRecordCommandHandler = updatePassengerNameRecordCommandHandler ?? throw new ArgumentNullException(nameof(updatePassengerNameRecordCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<OrderDocument>>> Handle(OrderChangeRequestData request, CancellationToken cancellationToken)
        {
            string token = null;
            string pcc = null;

            try
            {
                pcc = request?.Parties?.Participants?.FirstOrDefault()?.PccCode;

                token = await _createSessionCommandHandler.Handle(new CreateSessionCommand { PCC = pcc }, cancellationToken);

                if (token is null)
                {
                    return TechnicalFailure();
                }

                var getReservationRequest = GetReservationRequestMapper.GetReservationRequest(token, request.Rloc);
                var getReservationResponse = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRequest), cancellationToken);

                if (getReservationResponse?.Errors?.Any() == true || getReservationResponse?.Item is not ReservationPNRB)
                {
                    var errorMessage = string.Join(Environment.NewLine, getReservationResponse?.Errors?.Select(error => error.Message) ?? Enumerable.Empty<string>());
                    _logger.LogError("Sabre GetReservation returned an error response: {ErrorMessage}", errorMessage);

                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return TechnicalFailure();
                }

                var scheduleChangeEndTransactionResponse = await _enhancedEndTransactionCommandHandler.Handle(new EnhancedEndTransactionCommand { EnhancedEndTransactionRQ = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token, true) }, cancellationToken);

                if (scheduleChangeEndTransactionResponse?.ApplicationResults is null || scheduleChangeEndTransactionResponse.ApplicationResults.Error != null)
                {
                    var errorMessage = string.Join(Environment.NewLine, scheduleChangeEndTransactionResponse?.ApplicationResults?.Error?.Where(error => error.SystemSpecificResults != null).SelectMany(error => error.SystemSpecificResults).Where(result => result.Message != null).SelectMany(result => result.Message).Select(message => message.Value) ?? Enumerable.Empty<string>());
                    _logger.LogError("Sabre EnhancedEndTransaction returned an error response: {ErrorMessage}", errorMessage);
                    
                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return TechnicalFailure();
                }

                getReservationResponse = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRequest), cancellationToken);

                if (getReservationResponse?.Errors?.Any() == true || getReservationResponse?.Item is not ReservationPNRB)
                {
                    var errorMessage = string.Join(Environment.NewLine, getReservationResponse?.Errors?.Select(error => error.Message) ?? Enumerable.Empty<string>());
                    _logger.LogError("Sabre GetReservation returned an error response: {ErrorMessage}", errorMessage);

                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return TechnicalFailure();
                }

                var otaCancellationFlightItems = (getReservationResponse?.Item as ReservationPNRB)?.PassengerReservation?.Segments?.Segment?.Where(segment => segment.Item is SegmentTypePNRBSegmentAir airSegment && airSegment.ActionCode == "HK" && airSegment.PreviousActionCode == "TK").Select(segment => new FlightItems
                                                    {
                                                        ActionContext = FlightItems.ActionContexts.Delete,
                                                        Segments = [new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment
                                                    {
                                                        SegmentId = segment.sequence.ToString()
                                                    }]
                                                    }).ToArray();

                if (otaCancellationFlightItems?.Any() == true)
                {
                    var otaCancelRequest = OTACancelLLSRequestMapper.GetOTACancelRequest(token, otaCancellationFlightItems);
                    var otaCancelResponse = await _otaCancelLLSCommandHandler.Handle(new OTACancelLLSCommand { OtaCancelRequest = otaCancelRequest }, cancellationToken);

                    if (otaCancelResponse?.OTA_CancelRS?.ApplicationResults is null || otaCancelResponse.OTA_CancelRS.ApplicationResults.Error != null)
                    {
                        var errorMessage = string.Join(Environment.NewLine, otaCancelResponse?.OTA_CancelRS?.ApplicationResults?.Error?.Where(error => error.SystemSpecificResults != null).SelectMany(error => error.SystemSpecificResults).Where(result => result.Message != null).SelectMany(result => result.Message).Select(message => message.Value) ?? Enumerable.Empty<string>());
                        _logger.LogError("Sabre OTACancelLLS returned an error response: {ErrorMessage}", errorMessage);

                        await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                        return TechnicalFailure();
                    }
                }

                var enhancedEndTransactionResponse = await _enhancedEndTransactionCommandHandler.Handle(new EnhancedEndTransactionCommand { EnhancedEndTransactionRQ = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token, false) }, cancellationToken);

                if (enhancedEndTransactionResponse?.ApplicationResults is null || enhancedEndTransactionResponse.ApplicationResults.Error != null)
                {
                    var errorMessage = string.Join(Environment.NewLine, enhancedEndTransactionResponse?.ApplicationResults?.Error?.Where(error => error.SystemSpecificResults != null).SelectMany(error => error.SystemSpecificResults).Where(result => result.Message != null).SelectMany(result => result.Message).Select(message => message.Value) ?? Enumerable.Empty<string>());
                    _logger.LogError("Sabre EnhancedEndTransaction returned an error response: {ErrorMessage}", errorMessage);

                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return TechnicalFailure();
                }

                var updatePassengerNameRecordRQRequest = UpdatePassengerNameRecordRequestMapper.MapUpdatePassengerNameRecordRequest(request, token);
                var updatePassengerNameRecoreResponse = await _updatePassengerNameRecordCommandHandler.Handle(new UpdatePassengerNameRecordCommand { UpdatePassengerNameRecordRQRequest = updatePassengerNameRecordRQRequest }, cancellationToken);

                if (updatePassengerNameRecoreResponse?.UpdatePassengerNameRecordRS?.ApplicationResults is null ||
                    updatePassengerNameRecoreResponse.UpdatePassengerNameRecordRS.ApplicationResults.Error != null)
                {
                    var errorMessage = string.Join(Environment.NewLine, updatePassengerNameRecoreResponse?.UpdatePassengerNameRecordRS?.ApplicationResults?.Error?.Where(error => error.SystemSpecificResults != null).SelectMany(error => error.SystemSpecificResults).Where(result => result.Message != null).SelectMany(result => result.Message).Select(message => message.Value) ?? Enumerable.Empty<string>());
                    _logger.LogError("Sabre UpdatePassengerNameRecord returned an error response: {ErrorMessage}", errorMessage);

                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return TechnicalFailure();
                }

                getReservationResponse = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRequest), cancellationToken);

                if (getReservationResponse?.Errors?.Any() == true || getReservationResponse?.Item is not ReservationPNRB)
                {
                    var errorMessage = string.Join(Environment.NewLine, getReservationResponse?.Errors?.Select(error => error.Message) ?? Enumerable.Empty<string>());
                    _logger.LogError("Sabre GetReservation returned an error response: {ErrorMessage}", errorMessage);

                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return TechnicalFailure();
                }

                var airBookRequest = EnhancedAirBookRequestMapper.GetEnhancedAirBookRequest(token, request, request.DataList.CustomerDetails);
                var airBookResponse = await _enhancedAirBookCommandHandler.Handle(new EnhancedAirBookCommand { EnhancedAirBookRequest = airBookRequest }, cancellationToken);

                if (airBookResponse?.EnhancedAirBookRS?.ApplicationResults is null || airBookResponse.EnhancedAirBookRS.ApplicationResults.Error != null)
                {
                    var errorMessage = string.Join(Environment.NewLine, airBookResponse?.EnhancedAirBookRS?.ApplicationResults?.Error?.Where(error => error.SystemSpecificResults != null).SelectMany(error => error.SystemSpecificResults).Where(result => result.Message != null).SelectMany(result => result.Message).Select(message => message.Value) ?? Enumerable.Empty<string>());
                    _logger.LogError("Sabre EnhancedAirBook returned an error response: {ErrorMessage}", errorMessage);

                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return TechnicalFailure();
                }

                var enhancedEndTransactionRes = await _enhancedEndTransactionCommandHandler.Handle(new EnhancedEndTransactionCommand { EnhancedEndTransactionRQ = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token, false) }, cancellationToken);

                if (enhancedEndTransactionRes?.ApplicationResults?.Warning != null)
                {
                    var warningMessages = enhancedEndTransactionRes.ApplicationResults.Warning.Where(warning => warning.SystemSpecificResults != null).SelectMany(warning => warning.SystemSpecificResults).Where(result => result.Message != null).SelectMany(result => result.Message).Select(message => message.Value);

                    if (warningMessages.Any(message => message.Contains("IM AND CANCEL UNABLE SEGMENTS")))
                    {
                        _logger.LogError("Sabre EnhancedEndTransaction returned IM AND CANCEL UNABLE SEGMENTS.");

                        await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);

                        return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.IAmCancelError, AsyncContext.Current.CorrelationId.ToString(),EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }
                }

                if (enhancedEndTransactionRes?.ApplicationResults is null || enhancedEndTransactionRes.ApplicationResults.Error != null)
                {
                    var errorMessage = string.Join(Environment.NewLine, enhancedEndTransactionRes?.ApplicationResults?.Error?.Where(error => error.SystemSpecificResults != null).SelectMany(error => error.SystemSpecificResults).Where(result => result.Message != null).SelectMany(result => result.Message).Select(message => message.Value) ?? Enumerable.Empty<string>());
                    _logger.LogError("Sabre EnhancedEndTransaction returned an error response: {ErrorMessage}", errorMessage);

                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return TechnicalFailure();
                }

                await _designatePrinterCommandHandler.Handle(new DesignatePrinterCommand { DesignatePrinterRequest = DesignatePrinterRequestMapper.Map(token, pcc) }, cancellationToken);

                getReservationResponse = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRequest), cancellationToken);

                var passengerTickets = (getReservationResponse?.Item as ReservationPNRB)?.PassengerReservation?.Passengers?.Passenger? .SelectMany(passenger => passenger.TicketingInfo?.TicketDetails ?? Enumerable.Empty<TicketDetailsType>(), (passenger, ticket) => new { Ticket = ticket, NameId = passenger.nameId }).ToArray();

                if (getReservationResponse?.Errors?.Any() == true || getReservationResponse?.Item is not ReservationPNRB || passengerTickets?.Any() != true)
                {
                    var errorMessage = string.Join(Environment.NewLine, getReservationResponse?.Errors?.Select(error => error.Message) ?? Enumerable.Empty<string>());
                    if (errorMessage.IsNullOrWhiteSpace())
                    {
                        errorMessage = "Sabre GetReservation returned no passenger tickets.";
                    }

                    _logger.LogError("Sabre GetReservation returned an error response: {ErrorMessage}", errorMessage);

                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                    return TechnicalFailure();
                }

                foreach (var passengerTicket in passengerTickets)
                {
                    var ticketExchangeReq = TKTExchangeReqMapper.MapToTKTExchangeRQRequest(token, passengerTicket.Ticket.TicketNumber, pcc, passengerTicket.NameId);
                    await _tktExchangeCommand.Exchange(ticketExchangeReq);
                }

                getReservationResponse = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRequest), cancellationToken);

                var orderDocument = OrderDocumentMapper.Mapper(request, null, getReservationResponse);

                foreach (var ticket in orderDocument.OrderItems.SelectMany(x => x.AccountableDocumentRefs).DistinctBy(x => x.DocumentNumber))
                {
                    var couponResponse = await _getElectronicDocumentQueryHandler.Handle(new GetElectronicDocumentQuery() { GetElectronicDocumentRQRequest = GetElectronicDocumentRequestMapper.GetElectronicDocumentMapper(token, ticket) }, cancellationToken);
                    if (couponResponse?.DocumentDetailsDisplay != null)
                    {
                        orderDocument.OrderItems = orderDocument.OrderItems.Select(orderItem =>
                        {
                            var accountableDocumentRefsList = orderItem.AccountableDocumentRefs.ToList();
                            for (int i = 0; i < accountableDocumentRefsList.Count; i++)
                            {
                                if (accountableDocumentRefsList[i].DocumentNumber == ticket.DocumentNumber)
                                {
                                    accountableDocumentRefsList[i] = IssueTicketDataMapper.AccountableDocumentsMapper(ticket, couponResponse, orderDocument);
                                    break;
                                }
                            }
                            orderItem.AccountableDocumentRefs = accountableDocumentRefsList;
                            return orderItem;
                        });
                    }
                }

                await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);

                return Result.Success(new Response<OrderDocument>
                {
                    Status = Status.OK,
                    Value = orderDocument
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in OrderChangeReshopDisruption for {Rloc}", request?.Rloc);

                if (token is not null)
                {
                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
                }

                return TechnicalFailure();
            }
        }

        private static Result<Response<OrderDocument>> TechnicalFailure()
        {
            return Result.Failure<Response<OrderDocument>>(Json.Serialize(  ErrorMapper.MapTechnicalExceptionToEAPIException( EAPI.Infrastructure.Constants.Constants.TechnicalError,AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
        }

    }
}
