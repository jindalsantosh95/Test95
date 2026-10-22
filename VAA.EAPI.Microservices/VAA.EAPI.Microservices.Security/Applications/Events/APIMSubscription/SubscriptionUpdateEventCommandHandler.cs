using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription.Enums;
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Applications.Events.APIMSubscription
{
    public class SubscriptionUpdateEventCommandHandler : IRequestHandler<SubscriptionUpdateEventCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SubscriptionUpdateEventCommandHandler> _logger;


        public SubscriptionUpdateEventCommandHandler(IMediator mediator, ILogger<SubscriptionUpdateEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(SubscriptionUpdateEventCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var updateSubscriptionRequest = Json.Deserialize<SubscriptionUpdateCommand>(Convert.ToString(request.Data));
                var subscriptionUpdateResponse = await _mediator.Send(new UpdateAPIMSubscriptionRepoCommand(updateSubscriptionRequest), cancellationToken);
                if (subscriptionUpdateResponse.Status == Status.OK)
                {
                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = "Sucess"
                    };
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(subscriptionUpdateResponse), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), APIMSubscriptionCommandType.UpdateSubscription.ToString())) });
                    return result;
                }
                else
                {
                    var result = Result.Failure<Response<string>>(Json.Serialize(subscriptionUpdateResponse));
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(subscriptionUpdateResponse), Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), APIMSubscriptionCommandType.UpdateSubscription.ToString())) });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription update event processing failed for {EventId}.", request.Id);
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }

        }
    }
}
