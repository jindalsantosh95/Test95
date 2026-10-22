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
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMUser;

namespace VAA.EAPI.Microservices.Security.Applications.Events.APIMSubscription
{
    public class SubscriptionCreateEventCommandHandler : IRequestHandler<SubscriptionCreateEventCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SubscriptionCreateEventCommandHandler> _logger;


        public SubscriptionCreateEventCommandHandler(IMediator mediator, ILogger<SubscriptionCreateEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(SubscriptionCreateEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var subscriptionRequestData = Json.Deserialize<SubscriptionCreateCommand>(Convert.ToString(request.Data));
                string createdUserId = await _mediator.Send(new CreateAPIMUserRepoCommand(subscriptionRequestData), cancellationToken);
                subscriptionRequestData.UserAccount.UserId = createdUserId;
                var subscriptionCreateResponse = await _mediator.Send(new CreateAPIMSubscriptionRepoCommand(subscriptionRequestData), cancellationToken);
                if (subscriptionCreateResponse.Status == Status.OK)
                {
                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = "Sucess"
                    };
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(subscriptionCreateResponse), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), APIMSubscriptionCommandType.CreateSubscription.ToString())) });
                    return result;
                }
                else
                {
                    var result = Result.Failure<Response<string>>(Json.Serialize(subscriptionCreateResponse));
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(subscriptionCreateResponse), Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), APIMSubscriptionCommandType.CreateSubscription.ToString())) });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription create event processing failed for {EventId}.", request.Id);
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }

        }
    }
}
