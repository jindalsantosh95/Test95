using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
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

    public class SubscriptionUpdateCommandHandler : IRequestHandler<SubscriptionUpdateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SubscriptionUpdateCommandHandler> _logger;

        public SubscriptionUpdateCommandHandler(IMediator mediator, ILogger<SubscriptionUpdateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(SubscriptionUpdateCommand updateRequest, CancellationToken cancellationToken)
        {
            try
            {
                var cloudEvent = APIMSubscriptionCommandToEventMapper<SubscriptionUpdateCommand>.CommandToEventMapper(updateRequest, APIMSubscriptionEventType.UpdateAPIMSubscription, updateRequest.Channel, APIMSubscriptionCommandType.UpdateSubscription);
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                try
                {
                    await _mediator.Publish(cloudEvent);

                    var subscriptionUpdateResponse = APIMSubscriptionEventCommandResponseMapper.APIMSubscriptionEventResponse(cloudEvent.Id, Status.Accepted, Constants.Environment.ApimSubscriptionUpdateResponseUrl);
                    return subscriptionUpdateResponse != null ? Result.Success(subscriptionUpdateResponse) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
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
                _logger.LogError(ex, "Subscription update request processing failed.");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
