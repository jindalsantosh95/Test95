using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription.Enums;
using VAA.EAPI.Microservices.Security.Mapper.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription
{

    public class SubscriptionDeleteCommandHandler : IRequestHandler<SubscriptionDeleteCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SubscriptionDeleteCommandHandler> _logger;

        public SubscriptionDeleteCommandHandler(IMediator mediator, ILogger<SubscriptionDeleteCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(SubscriptionDeleteCommand deleteRequest, CancellationToken cancellationToken)
        {
            try
            {
                var cloudEvent = APIMSubscriptionCommandToEventMapper<SubscriptionDeleteCommand>.CommandToEventMapper(deleteRequest, APIMSubscriptionEventType.DeleteAPIMSubscription, deleteRequest.Channel, APIMSubscriptionCommandType.DeleteSubscription);
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                try
                {
                    await _mediator.Publish(cloudEvent);

                    return Result.Success(APIMSubscriptionEventCommandResponseMapper.APIMSubscriptionEventResponse(cloudEvent.Id, Status.Accepted, Constants.Environment.ApimSubscriptionDeleteResponseUrl));
                }
                catch
                {
                    _logger.LogDebug($"Deleting the event from event store");
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject, Source = cloudEvent.Source, Originator = cloudEvent.Originator }, cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription delete request processing failed.");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }

}
