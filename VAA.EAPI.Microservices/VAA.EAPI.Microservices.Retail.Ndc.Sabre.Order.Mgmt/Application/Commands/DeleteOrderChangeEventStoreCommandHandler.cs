using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    public class DeleteOrderEventStoreCommandHandler : IRequestHandler<DeleteOrderChangeEventStoreCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteOrderEventStoreCommandHandler> _logger;

        public DeleteOrderEventStoreCommandHandler(IMediator mediator, ILogger<DeleteOrderEventStoreCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(DeleteOrderChangeEventStoreCommand request, CancellationToken cancellationToken)
        {
            int retryRequestCounter = 1;
            int retryResponseCounter = 1;
            int retryLimit = ConfigReader.Get<int>(Constants.Environment.DeletionRetryCounterLimit);
            bool requestDel = false;
            bool responseDel = false;

            while (retryRequestCounter <= retryLimit && !requestDel)
            {
                try
                {
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = request.Id, Type = Constants.Application.OrderChangeEventType }, cancellationToken);
                    requestDel = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete event-store request record {EventId}", request.Id);
                    retryRequestCounter++;

                    if (ex.Message.Contains("Not Found")) requestDel = true;
                }
            }

            while (retryResponseCounter <= retryLimit && !responseDel)
            {
                try
                {
                    await _mediator.Send(new ServiceResponseRollBackCommand { Id = request.Id, Type = Constants.Application.OrderChangeEventSubject }, cancellationToken);
                    responseDel = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete event-store response record {EventId}", request.Id);
                    retryResponseCounter++;

                    if (ex.Message.Contains("Not Found")) responseDel = true;
                }
            }

            return Unit.Value;
        }
    }
}