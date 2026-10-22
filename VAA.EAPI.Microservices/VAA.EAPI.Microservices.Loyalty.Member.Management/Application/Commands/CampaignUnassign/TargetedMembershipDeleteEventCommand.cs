using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.CampaignUnassign
{
    public class TargetedMembershipDeleteEventCommand : CloudEvent, IRequest<Result<Response<string>>>
    {

    }

    public class TargetedMembershipDeleteEventCommandHandler : IRequestHandler<TargetedMembershipDeleteEventCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TargetedMembershipDeleteEventCommandHandler> _logger;

        public TargetedMembershipDeleteEventCommandHandler(IMediator mediator, ILogger<TargetedMembershipDeleteEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(TargetedMembershipDeleteEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Targeted Membership Delete Event Command Handler - Start");

                TargetedMembershipDeleteRequestData targetedMembershipDeleteRequest = Json.Deserialize<TargetedMembershipDeleteRequestData>(Convert.ToString(request.Data));
                var targetedMemberDeleteResponse = await _mediator.Send(new TargetedMembershipReportingCommand { CustomerId = targetedMembershipDeleteRequest.TargetedMembershipDocument.CustomerId, DocumentId = targetedMembershipDeleteRequest.TargetedMembershipDocument.Id });

                if (targetedMemberDeleteResponse.Value.Value.Equals(Constants.Application.Success))
                {
                    return new Response<string>
                    {
                        Status = Status.OK,
                        Value = Constants.Application.DeleteSuccessMessage
                    };
                }
                else
                {
                    _logger.LogInformation("Targeted Membership Delete Event Command Handler failed with error");
                    return Result.Failure<Response<string>>(targetedMemberDeleteResponse.Value.Value);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Targeted Membership Delete Event Command Handler failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
            finally
            {
                _logger.LogInformation($"Targeted Membership Delete Event Command Handler - End");
            }
        }
    }
}
