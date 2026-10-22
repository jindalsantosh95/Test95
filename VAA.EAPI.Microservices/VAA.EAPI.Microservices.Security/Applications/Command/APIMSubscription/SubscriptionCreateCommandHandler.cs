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
    public class SubscriptionCreateCommandHandler : IRequestHandler<SubscriptionCreateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SubscriptionCreateCommandHandler> _logger;

        public SubscriptionCreateCommandHandler(IMediator mediator, ILogger<SubscriptionCreateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(SubscriptionCreateCommand createRequest, CancellationToken cancellationToken)
        {
            try
            {
                var cloudEvent = APIMSubscriptionCommandToEventMapper<SubscriptionCreateCommand>.CommandToEventMapper(createRequest, APIMSubscriptionEventType.CreateAPIMSubscription, string.Empty, APIMSubscriptionCommandType.CreateSubscription);
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject, Source = cloudEvent.Source, Originator = cloudEvent.Originator });
                try
                {
                    await _mediator.Publish(cloudEvent);

                    return Result.Success(APIMSubscriptionEventCommandResponseMapper.APIMSubscriptionEventResponse(cloudEvent.Id, Status.Accepted, Constants.Environment.ApimSubscriptionCreateResponseUrl));
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
                _logger.LogError(ex, "Subscription create request processing failed.");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
