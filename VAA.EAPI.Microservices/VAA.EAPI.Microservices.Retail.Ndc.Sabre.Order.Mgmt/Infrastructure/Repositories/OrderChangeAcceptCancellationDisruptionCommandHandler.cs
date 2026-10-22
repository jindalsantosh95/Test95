using CSharpFunctionalExtensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories
{
    public class OrderChangeAcceptCancellationDisruptionCommandHandler : IOrderChangeAcceptCancellationDisruptionCommand
    {
        private readonly GetReservationQueryHandler _getReservationQueryHandler;
        private readonly CreateSessionCommandHandler _createSessionCommandHandler;
        private readonly EnhancedEndTransactionCommandHandler _enhancedEndTransactionCommandHandler;
        private readonly GetElectronicDocumentQueryHandler _getElectronicDocumentQueryHandler;
        private readonly SessionCloseCommandHandler _sessionCloseCommandHandler;
        private readonly DesignatePrinterCommandHandler _designatePrinterCommandHandler;
        private readonly ITKTExchangeCommand _tktExchangeCommand;

        public OrderChangeAcceptCancellationDisruptionCommandHandler(GetReservationQueryHandler getReservationQueryHandler, CreateSessionCommandHandler createSessionCommandHandler, EnhancedEndTransactionCommandHandler enhancedEndTransactionCommandHandler, GetElectronicDocumentQueryHandler getElectronicDocumentQueryHandler, SessionCloseCommandHandler sessionCloseCommandHandler, DesignatePrinterCommandHandler designatePrinterCommandHandler, ITKTExchangeCommand tKTExchangeCommand)
        {
            _getReservationQueryHandler = getReservationQueryHandler;
            _createSessionCommandHandler = createSessionCommandHandler;
            _enhancedEndTransactionCommandHandler = enhancedEndTransactionCommandHandler;
            _getElectronicDocumentQueryHandler = getElectronicDocumentQueryHandler;
            _sessionCloseCommandHandler = sessionCloseCommandHandler;
            _designatePrinterCommandHandler = designatePrinterCommandHandler;
            _tktExchangeCommand = tKTExchangeCommand;
        }

        public async Task<Result<Response<OrderDocument>>> Handle(OrderChangeRequestData request, CancellationToken cancellationToken)
        {
            var pcc = request.Parties.Participants.First().PccCode;
            OrderDocument orderDocument = null;
            GetReservationRS getReservationRS = null;

            var token = await _createSessionCommandHandler.Handle(new CreateSessionCommand() { PCC = pcc }, cancellationToken);

            if (token != null)
            {
                var getReservationRQ = GetReservationRequestMapper.GetReservationRequest(token, request.Rloc);
                getReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRQ), cancellationToken);

                if (getReservationRS != null)
                {
                    await _designatePrinterCommandHandler.Handle(new DesignatePrinterCommand() { DesignatePrinterRequest = DesignatePrinterRequestMapper.Map(token, request.Parties.Participants.First().PccCode) }, cancellationToken);

                    await _enhancedEndTransactionCommandHandler.Handle(new EnhancedEndTransactionCommand() { EnhancedEndTransactionRQ = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token , true) }, cancellationToken);

                    var ticketDetails = ((ReservationPNRB)getReservationRS.Item).PassengerReservation.TicketingInfo?.TicketDetails;

                    foreach(var ticket in ticketDetails ?? [])
                    {
                        var req = TKTExchangeReqMapper.MapToTKTExchangeRQRequest(token, ticket.TicketNumber, pcc,null);
                        await _tktExchangeCommand.Exchange(req);
                    }

                    getReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRQ), cancellationToken);

                    orderDocument = OrderDocumentMapper.DisruptionOrderDocMapper(getReservationRS, request);

                    foreach (var ticket in orderDocument.OrderItems
                        .Where(x => x.AccountableDocumentRefs?.Any() == true)
                        .SelectMany(x => x.AccountableDocumentRefs)
                        .DistinctBy(x => x.DocumentNumber))
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
                }
                else
                {
                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand() { BinaryToken = token, PCC = pcc }, cancellationToken);

                    return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }

                await _sessionCloseCommandHandler.Handle(new SessionCloseCommand() { BinaryToken = token, PCC = pcc }, cancellationToken);

                return Result.Success(new Response<OrderDocument>
                {
                    Status = Status.OK,
                    Value = orderDocument
                });
            }
            else
                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
        }
    }
}
