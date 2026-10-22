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
using VAA.EAPI.Microservices.Security.Infrastructure.Helper;
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Applications.Events.APIMSubscription
{
    public class SubscriptionDeleteEventCommandHandler : IRequestHandler<SubscriptionDeleteEventCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SubscriptionDeleteEventCommandHandler> _logger;


        public SubscriptionDeleteEventCommandHandler(IMediator mediator, ILogger<SubscriptionDeleteEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(SubscriptionDeleteEventCommand request, CancellationToken cancellationToken)
        {


            try
            {
                var deleteSubscriptionRequest = Json.Deserialize<SubscriptionDeleteCommand>(Convert.ToString(request.Data));
                var apimConfig = APIMSubscriptionConfigHelper.GetAPIMConfiguration(deleteSubscriptionRequest.Channel, deleteSubscriptionRequest.Environment);
                var uri = APIMSubscriptionConfigHelper.GetSubscriptionURI(apimConfig, deleteSubscriptionRequest.SubscriptionId);
                var subscriptionDeleteResponse = await _mediator.Send(new DeleteAPIMSubscriptionRepoCommand(uri, deleteSubscriptionRequest), cancellationToken);

                if (subscriptionDeleteResponse.Status == Status.OK)
                {
                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = "Sucess"
                    };
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(subscriptionDeleteResponse), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), APIMSubscriptionCommandType.DeleteSubscription.ToString())) });
                    return result;
                }
                else
                {
                    var result = Result.Failure<Response<string>>(Json.Serialize(subscriptionDeleteResponse));
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(subscriptionDeleteResponse), Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), APIMSubscriptionCommandType.DeleteSubscription.ToString())) });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription delete event processing failed for {EventId}.", request.Id);
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }

        }

    }
}
