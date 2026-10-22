using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirBook;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Mapper;
using Status = VAA.EAPI.Domain.Entities.Common.Status;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Application.Commands
{
    public class IssueTicketCommandhandler : IRequestHandler<IssueTicketCommand, Result<Response<IssueTicketResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IssueTicketCommandhandler> _logger;

        public IssueTicketCommandhandler(IMediator mediator, ILogger<IssueTicketCommandhandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IssueTicketResponse>>> Handle(IssueTicketCommand request, CancellationToken cancellationToken)
        {
            var id = string.Empty;
            try
            {
                var ticketResponse = await _mediator.Send(new IssueTicketRepoCommand() { IssueTicketRequest = request.IssueTicketRequest }, cancellationToken);

                if (ticketResponse.IsSuccess && ticketResponse.Value != null)
                {
                    ticketResponse.Value.Value.RequestType = request.IssueTicketRequest.RequestType;
                    ticketResponse.Value.Value.ChangeType = request.IssueTicketRequest.RequestType == "HoldBooking" ? ChangeTypeEnum.DocumentIssuance : ChangeTypeEnum.OrderItemAdded;
                    ticketResponse.Value.Value.PriceClass = request.IssueTicketRequest.PriceClass?.ToList() ?? new List<PriceClass>();
                    var issueTicketCloudEvent = UpdateTicketCloudEventMapper.EventMapper(ticketResponse.Value.Value);

                    id = issueTicketCloudEvent.Id;

                    await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = issueTicketCloudEvent, ServiceBusTopicName = Constants.Application.NDCUpdateTicketTopicName, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderDataServiceBusConnectionString) }, cancellationToken);

                    return new Response<IssueTicketResponse>() { Status = Status.OK, Value = ticketResponse.Value.Value };
                }

                return Result.Failure<Response<IssueTicketResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IssueTicket command failed. {EventId}", id);
                return Result.Failure<Response<IssueTicketResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}


