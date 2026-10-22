using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories
{
    public class OrderChangeReshopHoldBookingCommandHandler : IOrderChangeReshopHoldBookingCommand
    {
        private readonly CreateSessionCommandHandler _createSessionCommandHandler;
        private readonly GetReservationQueryHandler _getReservationQueryHandler;
        private readonly OTACancelLLSCommandHandler _oTACancelLLSCommandHandler;
        private readonly UpdatePassengerNameRecordCommandHandler _updatePassengerNameRecordCommandHandler;
        private readonly ManualPriceQuoteAddInfoCommandHandler _manualPriceQuoteAddInfoCommandHandler;
        private readonly EnhancedEndTransactionCommandHandler _enhancedEndTransactionCommandHandler;
        private readonly EnhancedAirTicketCommandHandler _enhancedAirTicketCommandHandler;
        private readonly SessionCloseCommandHandler _sessionCloseCommandHandler;
        private readonly GetElectronicDocumentQueryHandler _getElectronicDocumentQueryHandler;
        private readonly ILogger<OrderChangeReshopHoldBookingCommandHandler> _logger;

        public OrderChangeReshopHoldBookingCommandHandler(CreateSessionCommandHandler createSessionCommandHandler, GetReservationQueryHandler getReservationQueryHandler, OTACancelLLSCommandHandler oTACancelLLSCommandHandler, UpdatePassengerNameRecordCommandHandler updatePassengerNameRecordCommandHandler, ManualPriceQuoteAddInfoCommandHandler manualPriceQuoteAddInfoCommandHandler, EnhancedEndTransactionCommandHandler enhancedEndTransactionCommandHandler, EnhancedAirTicketCommandHandler enhancedAirTicketCommandHandler, SessionCloseCommandHandler sessionCloseCommandHandler, GetElectronicDocumentQueryHandler getElectronicDocumentQueryHandler, ILogger<OrderChangeReshopHoldBookingCommandHandler> logger)
        {
            _createSessionCommandHandler = createSessionCommandHandler;
            _getReservationQueryHandler = getReservationQueryHandler;
            _oTACancelLLSCommandHandler = oTACancelLLSCommandHandler;
            _updatePassengerNameRecordCommandHandler = updatePassengerNameRecordCommandHandler;
            _manualPriceQuoteAddInfoCommandHandler = manualPriceQuoteAddInfoCommandHandler;
            _enhancedEndTransactionCommandHandler = enhancedEndTransactionCommandHandler;
            _enhancedAirTicketCommandHandler = enhancedAirTicketCommandHandler;
            _sessionCloseCommandHandler = sessionCloseCommandHandler;
            _getElectronicDocumentQueryHandler = getElectronicDocumentQueryHandler;
            _logger = logger;
        }

        public async Task<Result<Response<OrderDocument>>> Handle(OrderChangeRequestData request, CancellationToken cancellationToken)
        {
            string token = null;
            var pcc = request.Parties.Participants.First().PccCode;
            OrderDocument orderDocument = null;
            GetReservationRS getReservationRS = null;
            AirTicketRS enhancedAirTicketRs = null;

            try
            {
                token = await _createSessionCommandHandler.Handle(new CreateSessionCommand { PCC = pcc }, cancellationToken);

                var getReservationRQ = GetReservationRequestMapper.GetReservationRequest(token, request.Rloc);
                getReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRQ), cancellationToken);

                if (getReservationRS == null || getReservationRS.Errors != null)
                {
                    await FailAndClose("Failure in GetReservation", token, pcc, cancellationToken);
                    return TechnicalFailure();
                }

                System.Collections.Generic.IEnumerable<FlightItems> cancellationFlightItems;
                if (request.IsReshopDisruption)
                {
                    var scheduleChangeEndTransactionRq = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token, isScheduleChangeInd: true);
                    var scheduleChangeEndTransactionRs = await _enhancedEndTransactionCommandHandler.Handle(new EnhancedEndTransactionCommand { EnhancedEndTransactionRQ = scheduleChangeEndTransactionRq }, cancellationToken);

                    if (scheduleChangeEndTransactionRs?.ApplicationResults == null || scheduleChangeEndTransactionRs.ApplicationResults.Error != null)
                    {
                        await FailAndClose("Failure in ScheduleChangeEnhancedEndTransaction", token, pcc, cancellationToken);
                    }

                    getReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRQ), cancellationToken);

                    if (getReservationRS == null || getReservationRS.Errors != null)
                    {
                        await FailAndClose("Failure in GetReservation", token, pcc, cancellationToken);
                    }

                    cancellationFlightItems = (getReservationRS.Item as ReservationPNRB)?.PassengerReservation?.Segments?.Segment
                        ?.Where(segment => segment.Item is SegmentTypePNRBSegmentAir airSegment &&
                                          airSegment.ActionCode == "HK" &&
                                          airSegment.PreviousActionCode == "TK")
                        .Select(segment => new FlightItems
                        {
                            ActionContext = FlightItems.ActionContexts.Delete,
                            Segments = [new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment
                            {
                                SegmentId = segment.sequence.ToString()
                            }]
                        }).ToArray();
                }
                else
                {
                    cancellationFlightItems = request.OrderAmendments.First().FlightItems;
                }

                if (cancellationFlightItems?.Any() == true)
                {
                    var cancelReq = OTACancelLLSRequestMapper.GetOTACancelRequest(token, cancellationFlightItems);
                    var cancelRes = await _oTACancelLLSCommandHandler.Handle(new OTACancelLLSCommand { OtaCancelRequest = cancelReq }, cancellationToken);

                    if (cancelRes?.OTA_CancelRS.ApplicationResults?.Error != null)
                    {
                        await FailAndClose("Failure in OTACancelLLS", token, pcc, cancellationToken);
                    }
                }

                await ExecuteEOT(token, false, cancellationToken);

                var updatePassengerNameRecordRq = UpdatePassengerNameRecordRqMapper.Map(token, request);
                var updatePassengerNameRecordRs = await _updatePassengerNameRecordCommandHandler.Handle(new UpdatePassengerNameRecordCommand { UpdatePassengerNameRecordRQRequest = updatePassengerNameRecordRq }, cancellationToken);

                if (updatePassengerNameRecordRs == null || updatePassengerNameRecordRs.UpdatePassengerNameRecordRS.ApplicationResults?.Error != null)
                {
                    var hasUnableSegmentError = updatePassengerNameRecordRs?.UpdatePassengerNameRecordRS?.ApplicationResults?.Warning? .Any(w => w.SystemSpecificResults?  .Any(s => s.Message?.Any(m => m.Value.Contains("*IM AND CANCEL UNABLE SEGMENTS")) == true) == true) == true;
                    if (hasUnableSegmentError)
                    {
                        await CloseSession(token, pcc, cancellationToken);
                        return Result.Failure<Response<OrderDocument>>( Json.Serialize( ErrorMapper.MapTechnicalExceptionToEAPIException( Constants.ErrorCodes.InValidSegmentError,  Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }

                    await FailAndClose("Failure in UpdatePassengerNameRecord", token, pcc, cancellationToken);
                }

                getReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRQ), cancellationToken);

                if (getReservationRS == null || getReservationRS.Errors != null)
                {
                    await FailAndClose("Failure in GetReservation", token, pcc, cancellationToken);
                }

                if (request.IsChangeReshopConfirmHoldBooking)
                {
                    var passengers = ((ReservationPNRB)getReservationRS.Item).PassengerReservation.Passengers.Passenger;

                    foreach (var ptcType in request.DataList.CustomerDetails.GroupBy(x => x.Ptc).Select((x, i) => new { Value = x.Key, Index = i }))
                    {
                        var manualPriceQuotecreateRQ = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, false, request, ptcType.Value, ptcType.Index.ToString(), passengers);
                        var manualPriceQuotecreateRS = await _manualPriceQuoteAddInfoCommandHandler.Handle(new ManualPriceQuoteAddInfoCommand { ManualPriceQuoteRequest = manualPriceQuotecreateRQ }, cancellationToken);

                        if (manualPriceQuotecreateRS == null || manualPriceQuotecreateRS.ApplicationResults?.Error != null)
                        {
                            await FailAndClose("Failure in ManualPriceQuoteCreate", token, pcc, cancellationToken);
                        }
                        var pqValue = manualPriceQuotecreateRS.Text.First().Split(" ")[1];

                        var manualPriceQuotecontentRQ = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, true, request, ptcType.Value, pqValue, passengers);

                        var manualPriceQuotecontentRS = await _manualPriceQuoteAddInfoCommandHandler.Handle(new ManualPriceQuoteAddInfoCommand { ManualPriceQuoteRequest = manualPriceQuotecontentRQ }, cancellationToken);

                        if (manualPriceQuotecontentRS == null || manualPriceQuotecontentRS.ApplicationResults?.Error != null)
                        {
                            await FailAndClose("Failure in ManualPriceQuoteContent", token, pcc, cancellationToken);
                        }
                    }

                    await ExecuteEOT(token, false, cancellationToken);

                    var enhancedAirTicketRq = AirTicketRequestMapper.GetAirticketRequest(token, request, pcc);
                    enhancedAirTicketRs = await _enhancedAirTicketCommandHandler.Handle(new EnhancedAirTicketCommand { EnhancedAirTicketRequest = enhancedAirTicketRq }, cancellationToken);

                    if (enhancedAirTicketRs == null || enhancedAirTicketRs.ApplicationResults?.Error != null)
                    {
                        await FailAndClose("Failure in EnhancedAirTicket", token, pcc, cancellationToken);
                    }

                }

                getReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRQ), cancellationToken);

                if (getReservationRS == null || getReservationRS.Errors != null)
                {
                    await FailAndClose("Failure in GetReservation", token, pcc, cancellationToken);
                }

                orderDocument = OrderDocumentMapper.Mapper(request, enhancedAirTicketRs, getReservationRS);

                if (request.IsChangeReshopConfirmHoldBooking)
                {
                    foreach (var ticket in orderDocument.OrderItems.SelectMany(x => x.AccountableDocumentRefs).DistinctBy(x => x.DocumentNumber))
                    {
                        var couponResponse = await _getElectronicDocumentQueryHandler.Handle(new GetElectronicDocumentQuery { GetElectronicDocumentRQRequest = GetElectronicDocumentRequestMapper.GetElectronicDocumentMapper(token, ticket) }, cancellationToken);

                        if (couponResponse?.DocumentDetailsDisplay != null)
                        {
                            orderDocument.OrderItems = orderDocument.OrderItems.Select(orderItem =>
                            {
                                var list = orderItem.AccountableDocumentRefs.ToList();

                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (list[i].DocumentNumber == ticket.DocumentNumber)
                                    {
                                        list[i] = IssueTicketDataMapper.AccountableDocumentsMapper(ticket, couponResponse, orderDocument);
                                        break;
                                    }
                                }

                                orderItem.AccountableDocumentRefs = list;
                                return orderItem;
                            });
                        }
                    }

                }

                await CloseSession(token, pcc, cancellationToken);

                return Result.Success(new Response<OrderDocument>
                {
                    Status = Status.OK,
                    Value = orderDocument
                });
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(token))
                {
                    await CloseSession(token, pcc, cancellationToken);
                }

                _logger.LogError(ex, "Unhandled exception in OrderChangeReshopHoldBooking");

                return TechnicalFailure();
            }
        }

        private async Task ExecuteEOT(string token, bool actionCode, CancellationToken cancellationToken)
        {
            var rq = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token, actionCode);
            await _enhancedEndTransactionCommandHandler.Handle(new EnhancedEndTransactionCommand { EnhancedEndTransactionRQ = rq }, cancellationToken);
        }

        private async Task CloseSession(string token, string pcc, CancellationToken cancellationToken)
        {
            await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = pcc }, cancellationToken);
        }

        private async Task FailAndClose(string message, string token, string pcc, CancellationToken cancellationToken)
        {
            _logger.LogError("Sabre order change reshop hold booking operation {Operation} returned an error response", message);

            await CloseSession(token, pcc, cancellationToken);

            throw new Exception(message);
        }

        private Result<Response<OrderDocument>> TechnicalFailure()
        {
            return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
        }
    }
}
