using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Repositories;
using TicketRetrieveResponseType = VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands
{
    public class OrderHistoryCommandHandler : IOrderHistoryCommand
    {
        private readonly ILogger<OrderHistoryCommandHandler> _logger;
        private readonly OrderSearchRepoCommandHandler _orderSearchRepoCommandHandler;
        private readonly PNRRetrieveQueryHandler _pNRRetrieveQueryHandler;
        private readonly TicketingRetrieveRepoHandler _ticketingRetrieveRepoHandler;
        private readonly IssueTicketRepoCommandHandler _issueTicketRepoCommandHandler;

       public OrderHistoryCommandHandler(ILogger<OrderHistoryCommandHandler> logger, OrderSearchRepoCommandHandler orderSearchRepoCommandHandler, PNRRetrieveQueryHandler pNRRetrieveQueryHandler, TicketingRetrieveRepoHandler ticketingRetrieveRepoHandler, IssueTicketRepoCommandHandler issueTicketRepoCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderSearchRepoCommandHandler = orderSearchRepoCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchRepoCommandHandler));
            _pNRRetrieveQueryHandler = pNRRetrieveQueryHandler ?? throw new ArgumentNullException(nameof(pNRRetrieveQueryHandler));
            _ticketingRetrieveRepoHandler = ticketingRetrieveRepoHandler ?? throw new ArgumentNullException(nameof(ticketingRetrieveRepoHandler));
            _issueTicketRepoCommandHandler = issueTicketRepoCommandHandler;
        }

        public async Task<Result<Response<OrderDocument>>> Handle(OrderSearchRequest request, CancellationToken cancellationToken)
        {

            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                var response = await _orderSearchRepoCommandHandler.Handle(new OrderSearchRepoCommand() { OrderSearchRequest = request }, cancellationToken);

                if (response.IsSuccess && response.Value.Status == Status.OK)
                {
                    var getReservationRS = response.Value.Value;
                    var reservation = (ReservationPNRB)getReservationRS.Item;
                    var getDeltaSeatServiceRS = reservation.PassengerReservation.Passengers.Passenger.Any(x => x.Seats?.PreReservedSeats is not null) is true ? await MapTktDeltaService(reservation) : null;
                    var getDeltaSeatTicketRS = reservation.PassengerReservation.Passengers.Passenger.Any(x => x.Seats?.PreReservedSeats is not null) is true ? await MapEMDDeltaService(reservation, cancellationToken) : null;

                    var orderDocument = SabreResponseToOrderDocumentMapper.MapForOrderHistory(request, getReservationRS, getDeltaSeatServiceRS, getDeltaSeatTicketRS);

                    var ticketDetails = ((ReservationPNRB)getReservationRS.Item).PassengerReservation.TicketingInfo.TicketDetails;

                    if (ticketDetails != null && ticketDetails.Any())
                    {
                        var issueTicketRequest = new IssueTicketRequest() { OrderDocument = orderDocument };
                        var updatedDocument = await _issueTicketRepoCommandHandler.Handle(new IssueTicketRepoCommand() { IssueTicketRequest = issueTicketRequest }, cancellationToken);
                        orderDocument.OrderItems = updatedDocument.Value.Value.OrderItems;
                    }
                    else
                    {
                        foreach (var segment in orderDocument.DataList.SegmentDetails)
                        {
                            var matchingSegment = request.SearchFilterCriteria.SegmentDetails.First(a => a.OriginAirportCode == segment.OriginAirportCode && a.DestinationAirportCode == segment.DestinationAirportCode);
                            segment.TicketNotValidAfter = matchingSegment.TicketNotValidAfter;
                            segment.TicketNotValidBefore = matchingSegment.TicketNotValidBefore;
                        }
                    }

                    return new Response<OrderDocument>() { Status = Status.OK, Value = orderDocument };
                }
                else
                {
                    return new Response<OrderDocument>()
                    {
                        Status = Status.BadRequest,
                        Errors = response.Value.Value.Errors.Select(x => new ErrorDetails() { Code = x.Code, Message = x.Message }).ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order history processing failed. {Operation}", "OrderHistory");

                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        private async Task<Result<Response<VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType>>> MapTktDeltaService(ReservationPNRB getReservationRS)
        {
            var deltaPnr = ((SegmentTypePNRBSegmentAir)getReservationRS.PassengerReservation.Segments.Segment.First().Item).AirlineRefId.Split("*").Last();
            var serviceResponse = await _pNRRetrieveQueryHandler.Handle(new PNRRetrieveQuery { PNR = deltaPnr }, CancellationToken.None);

            return serviceResponse;
        }

        private async Task<Result<Response<List<TicketRetrieveResponseType>>>> MapEMDDeltaService(ReservationPNRB getReservationRS, CancellationToken cancellationToken)
        {
            var deltaPnr = ((SegmentTypePNRBSegmentAir)getReservationRS.PassengerReservation.Segments.Segment.First().Item).AirlineRefId.Split("*").Last();
            var pnrRetrieveOutput = await _pNRRetrieveQueryHandler.Handle(new PNRRetrieveQuery { PNR = deltaPnr }, CancellationToken.None);

            TicketRetrieveResponseType ticketingResponse = null;
            List<TicketRetrieveResponseType> ticketServiceResponseList = [];
            List<TicketRetrieveResponseType> allTicketServiceResponseList = [];

            var tasks = pnrRetrieveOutput.Value.Value.TeardownContextResponse.Pnr.TicketInformation.Where(x => x.TicketNumber is not null).Select(async tkt =>
            {
                ticketingResponse = await GetTicketServiceResponse(tkt.TicketNumber, cancellationToken);

                if (tkt.TicketStatus == Constants.Application.HookedTicketStatus) ticketServiceResponseList.Add(ticketingResponse); allTicketServiceResponseList.Add(ticketingResponse);

                var feeDocumentIssuedTypes = ticketingResponse.TicketRequestResponse?.FirstOrDefault().TicketResponse?.FirstOrDefault()?.FeeDocumentIssued?.Where(x => x.FeeDocumentNumber is not null && x.FeeDocumentNumber.Any())?.Select(x => x);

                if (feeDocumentIssuedTypes != null && feeDocumentIssuedTypes.Any())
                {
                    var emdTasks = feeDocumentIssuedTypes.Select(async x =>
                    {
                        var emdTicketResponse = await GetTicketServiceResponse(x.FeeDocumentNumber, cancellationToken);
                        allTicketServiceResponseList.Add(emdTicketResponse);
                    });
                    await Task.WhenAll(emdTasks);
                }
            });
            await Task.WhenAll(tasks);

            return new Response<List<TicketRetrieveResponseType>>() { Status = Status.OK, Value = allTicketServiceResponseList };
        }

        private async Task<TicketRetrieveResponseType> GetTicketServiceResponse(string ticketNumber, CancellationToken cancellationToken) => await _ticketingRetrieveRepoHandler.Handle(new TicketingRetrieveRepoQuery(TicketingRetrieveMapper.TicketingRetrieveRequestMapper(ticketNumber)), cancellationToken);
    }
}



