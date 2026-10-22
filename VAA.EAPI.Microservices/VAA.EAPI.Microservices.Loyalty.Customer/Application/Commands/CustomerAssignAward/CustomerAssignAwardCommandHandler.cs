using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository.EventStore;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward
{
    public class CustomerAssignAwardCommandHandler : IRequestHandler<CustomerAssignAwardCommand, Result<Response<string>>>
    {
        private readonly ILogger<CustomerAssignAwardCommandHandler> _logger;
        private readonly IMediator _mediator;

        public CustomerAssignAwardCommandHandler(IMediator mediator, ILogger<CustomerAssignAwardCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(CustomerAssignAwardCommand request, CancellationToken cancellationToken)
        {

            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                _logger.LogInformation($"Store Event in event store - Start");
                await _mediator.Send(new EventRequestInsertCommand { Id = request.CloudEvent.Id, Type = request.CloudEvent.Type, Data = request.CloudEvent.Data, Subject = request.CloudEvent.Subject }, cancellationToken);
                _logger.LogInformation($"Store Event in event store - End");

                _logger.LogInformation($"Publish Service Bus Message- Start");
                await _mediator.Publish(request.CloudEvent, cancellationToken);
                _logger.LogInformation($"Publish Service Bus Message- End");
                return Result.Success(new Response<string> { Status = Status.Accepted });
            }

            catch (Exception ex)
            {
                _logger.LogInformation($"Rollback from event store- Start");
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = request.CloudEvent.Id, Type = request.CloudEvent.Type, Data = request.CloudEvent.Data, Subject = request.CloudEvent.Subject }, cancellationToken);
                _logger.LogInformation($"Rollback from event store- End");

                return Result.Failure<Response<string>>(Convert.ToString(ex.Message));
            }
        }
    }
}
