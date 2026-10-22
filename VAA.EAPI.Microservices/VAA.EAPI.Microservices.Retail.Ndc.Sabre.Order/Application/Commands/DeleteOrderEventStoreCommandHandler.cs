using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands
{
    public class DeleteOrderEventStoreCommandHandler : IRequestHandler<DeleteOrderEventStoreCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteOrderEventStoreCommandHandler> _logger;

        public DeleteOrderEventStoreCommandHandler(IMediator mediator, ILogger<DeleteOrderEventStoreCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(DeleteOrderEventStoreCommand request, CancellationToken cancellationToken)
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
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = request.Id, Type = Constants.Application.OrderCreateEventType }, cancellationToken);
                    requestDel = true;
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex, "Event-store request deletion attempt failed. {EventId} {Attempt}", request.Id, retryRequestCounter);
                    retryRequestCounter++;

                    if (ex.Message.Contains("Not Found")) requestDel = true;
                }
            }

            while (retryResponseCounter <= retryLimit && !responseDel)
            {
                try
                {
                    await _mediator.Send(new ServiceResponseRollBackCommand { Id = request.Id, Type = Constants.Application.OrderCreateEventSubject }, cancellationToken);
                    responseDel = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Service response rollback attempt failed. {EventId} {Attempt}", request.Id, retryResponseCounter);
                    retryResponseCounter++;

                    if (ex.Message.Contains("Not Found")) responseDel = true;
                }
            }

            if (!requestDel || !responseDel)
            {
                _logger.LogError("Event-store cleanup did not complete after retries. {EventId}", request.Id);
            }

            return Unit.Value;
        }
    }
}
