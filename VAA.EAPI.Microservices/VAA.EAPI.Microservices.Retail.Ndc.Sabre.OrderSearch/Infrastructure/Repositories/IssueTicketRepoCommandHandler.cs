using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Repositories
{
    public class IssueTicketRepoCommandHandler : IRequestHandler<IssueTicketRepoCommand, Result<Response<OrderDocument>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IssueTicketRepoCommandHandler> _logger;

        public IssueTicketRepoCommandHandler(IMediator mediator, ILogger<IssueTicketRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<OrderDocument>>> Handle(IssueTicketRepoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _mediator.Send(new CreateSessionCommand() { PCC = request.IssueTicketRequest.OrderDocument.Parties.Participants.First().PccCode}, cancellationToken);

                if (token != null)
                {
                    var segmentDict = request.IssueTicketRequest.OrderDocument.DataList.SegmentDetails.ToDictionary(s => (s.OriginAirportCode, s.DestinationAirportCode, s.ScheduledDepartureTime.LocalDateTime));

                    foreach (var ticket in request.IssueTicketRequest.OrderDocument.OrderItems.Where(x => x.AccountableDocumentRefs is not null).SelectMany(x => x.AccountableDocumentRefs).DistinctBy(x => x.DocumentNumber))
                    {
                        if (ticket.DocumentNumber is not null)
                        {
                            var couponRespose = await _mediator.Send(new GetElectronicDocumentQuery() { GetElectronicDocumentRQRequest = GetElectronicDocumentRQMapper.GetElectronicDocumentRQRequest(token, ticket) }, cancellationToken);

                            if (couponRespose?.DocumentDetailsDisplay != null)
                            {
                                request.IssueTicketRequest.OrderDocument.OrderItems = request.IssueTicketRequest.OrderDocument.OrderItems.Select(orderItem =>
                                {
                                    var accountableDocumentRefsList = orderItem.AccountableDocumentRefs?.ToList();

                                    if (accountableDocumentRefsList is not null)
                                    {
                                        for (int i = 0; i < accountableDocumentRefsList.Count; i++)
                                        {
                                            if (accountableDocumentRefsList[i].DocumentNumber == ticket.DocumentNumber)
                                            {
                                                accountableDocumentRefsList[i] = IssueTicketDataMapper.AccountableDocumentsMapper(ticket, couponRespose, request.IssueTicketRequest.OrderDocument);
                                                break;
                                            }
                                        }
                                    }
                                    orderItem.AccountableDocumentRefs = accountableDocumentRefsList;
                                    return orderItem;
                                });

                                var coupon = (TicketingDocumentTicketED)couponRespose.DocumentDetailsDisplay.Item;

                                foreach (var item in coupon.ServiceCoupon.Where(x=> x.FareBasis != null))
                                {
                                    if (segmentDict.TryGetValue((item.StartLocation.Value, item.EndLocation.Value, (item.StartDate.Date + item.StartTime.TimeOfDay).ToString("yyyy-MM-ddTHH:mm:ss")), out var segment))
                                    {
                                        segment.TicketNotValidAfter = item.NotValidAfterDate != DateTime.MinValue ? item.NotValidAfterDate.ToString("yyyy-MM-dd") : null;
                                        segment.TicketNotValidBefore = item.NotValidBeforeDate != DateTime.MinValue ? item.NotValidBeforeDate.ToString("yyyy-MM-dd") : null;
                                    }
                                }

                            }
                        }
                    }

                    await _mediator.Send(new SessionCloseCommand() { BinaryToken = token , PCC = request.IssueTicketRequest.OrderDocument.Parties.Participants.First().PccCode }, cancellationToken);

                    return Result.Success(new Response<OrderDocument>
                    {
                        Status = Status.OK,
                        Value = request.IssueTicketRequest.OrderDocument
                    });
                }

                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));

            }
            catch (ExternalServiceIntegrationException)
            {
                _logger.LogError("Order search ticket enrichment failed. {Operation}", "GetElectronicDocument");
                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order search ticket enrichment failed. {Operation}", "GetElectronicDocument");
                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
