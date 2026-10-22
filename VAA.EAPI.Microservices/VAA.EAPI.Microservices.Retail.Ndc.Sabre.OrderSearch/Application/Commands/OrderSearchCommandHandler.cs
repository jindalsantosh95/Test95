using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Repositories;
using VAA.EAPI.Infrastructure.PSS.Repository;
using TicketRetrieveResponseType = VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType;
using VAA.EAPI.Infrastructure.PSS.Proxy.HostContext;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands
{
    public class OrderSearchCommandHandler : IRequestHandler<OrderSearchCommand, Result<Response<OrderDocument>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderSearchCommandHandler> _logger;

        public OrderSearchCommandHandler(IMediator mediator, ILogger<OrderSearchCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<OrderDocument>>> Handle(OrderSearchCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.OrderSearchRequest == null) throw new ArgumentNullException(nameof(request.OrderSearchRequest));

                var response = await _mediator.Send(new OrderSearchRepoCommand() { OrderSearchRequest = request.OrderSearchRequest }, cancellationToken);

                if (response.IsSuccess && response.Value.Status == Status.OK)
                {
                    var getReservationRS = response.Value.Value;
                    var reservation = (ReservationPNRB)getReservationRS.Item;
                    var hasPreReservedSeats = reservation.PassengerReservation.Passengers.Passenger.Any(x => x.Seats?.PreReservedSeats is not null);
                    var getDeltaSeatServiceRS = hasPreReservedSeats ? await MapTktDeltaService(reservation): null;

                    if (getDeltaSeatServiceRS.IsFailure == true && getDeltaSeatServiceRS.Error.Contains("500325", StringComparison.OrdinalIgnoreCase))
                    {
                        return Air4NotFoundResponse();
                    }

                    var getDeltaSeatTicketRS = hasPreReservedSeats ? await MapEMDDeltaService(reservation, cancellationToken) : null;

                    if (getDeltaSeatTicketRS.IsFailure == true && getDeltaSeatTicketRS.Error.Contains("Air4 is not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return Air4NotFoundResponse();
                    }

                    var orderDocument = !request.OrderSearchRequest.SearchFilterCriteria.HistoryFlag ? SabreResponseToOrderDocumentMapper.Map(request.OrderSearchRequest, getReservationRS, getDeltaSeatServiceRS, getDeltaSeatTicketRS) : SabreResponseToOrderDocumentMapper.MapForOCN(request.OrderSearchRequest, getReservationRS, getDeltaSeatServiceRS, getDeltaSeatTicketRS);
                    var ticketDetails = ((ReservationPNRB)getReservationRS.Item).PassengerReservation.TicketingInfo.TicketDetails;

                    if (ticketDetails != null && ticketDetails.Any())
                    {
                        var issueTicketRequest = new IssueTicketRequest() { OrderDocument = orderDocument };
                        var updatedDocument = await _mediator.Send(new IssueTicketRepoCommand() { IssueTicketRequest = issueTicketRequest }, cancellationToken);
                        orderDocument.OrderItems = updatedDocument.Value.Value.OrderItems;
                    }
                    else
                    {
                        foreach (var segment in orderDocument.DataList.SegmentDetails)
                        {
                            var matchingSegment = request.OrderSearchRequest.SearchFilterCriteria.SegmentDetails.First(a => a.OriginAirportCode == segment.OriginAirportCode && a.DestinationAirportCode == segment.DestinationAirportCode);
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
                _logger.LogError(ex, "Order search processing failed. {Operation}", "OrderSearch");

                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        private static Response<OrderDocument> Air4NotFoundResponse() => new()
        {
            Status = Status.BusinessException,
            Errors =
            [
                new ErrorDetails
                {
                    Code = "500325",
                    Message = "Air4 reservation was not found."
                }
            ]
        };

        private async Task<Result<Response<VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType>>> MapTktDeltaService(ReservationPNRB getReservationRS)
        {
            try
            {
                var deltaPnr = ((SegmentTypePNRBSegmentAir)getReservationRS.PassengerReservation.Segments.Segment.First().Item).AirlineRefId.Split("*").Last();
                var serviceResponse = await _mediator.Send(new PNRRetrieveQuery { PNR = deltaPnr });

                return serviceResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Air4 PNR retrieval failed.");
                return Result.Failure<Response<VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType>>("500325");
            }
        }

        private async Task<Result<Response<List<TicketRetrieveResponseType>>>> MapEMDDeltaService(ReservationPNRB getReservationRS, CancellationToken cancellationToken)
        {
            try
            {
                var deltaPnr = ((SegmentTypePNRBSegmentAir)getReservationRS.PassengerReservation.Segments.Segment.First().Item).AirlineRefId.Split("*").Last();
                var pnrRetrieveOutput = await _mediator.Send(new PNRRetrieveQuery { PNR = deltaPnr });

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
                        foreach (var feeDocIssuedType in feeDocumentIssuedTypes)
                        {
                            var emdTicketResponse = await GetTicketServiceResponse(feeDocIssuedType.FeeDocumentNumber, cancellationToken);
                            allTicketServiceResponseList.Add(emdTicketResponse);
                        }
                    }
                });
                await Task.WhenAll(tasks);

                return new Response<List<TicketRetrieveResponseType>>() { Status = Status.OK, Value = allTicketServiceResponseList };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Air4 PNR retrieval failed.");
                return Result.Failure<Response<List<TicketRetrieveResponseType>>>("Air4 PNR retrieval failed");
            }
        }

        private async Task<TicketRetrieveResponseType> GetTicketServiceResponse(string ticketNumber, CancellationToken cancellationToken) => await _mediator.Send(new TicketingRetrieveRepoQuery(TicketingRetrieveMapper.TicketingRetrieveRequestMapper(ticketNumber)), cancellationToken);
    }
}
