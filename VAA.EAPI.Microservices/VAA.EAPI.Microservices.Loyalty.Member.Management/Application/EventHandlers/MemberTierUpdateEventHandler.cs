using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers
{
    public class MemberTierUpdateEventHandler : IRequestHandler<MemberTierUpdateEvent, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MemberTierUpdateEventHandler> _logger;

        public MemberTierUpdateEventHandler(IMediator mediator,
            ILogger<MemberTierUpdateEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(MemberTierUpdateEvent request, CancellationToken cancellationToken)
        {
            try
            {
                MemberTierUpdateEventData tierUpdateEvent = Json.Deserialize<MemberTierUpdateEventData>(Convert.ToString(request.Data));

                if (tierUpdateEvent == null ||
                   string.IsNullOrWhiteSpace(tierUpdateEvent.InitiatedBy) ||
                   string.IsNullOrWhiteSpace(tierUpdateEvent.CustomerId) ||
                   string.IsNullOrWhiteSpace(tierUpdateEvent.NewTier) ||
                   string.IsNullOrWhiteSpace(tierUpdateEvent.OldTier))
                {
                    _logger.LogError($"MemberTierUpdateEvent - Invalid request. Attributes 'InitiatedBy', 'CustomerId', 'NewTier' and 'OldTier' are mandatory");
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                await _mediator.Send(new SSADelinkOnTierUpdateCommand()
                {
                    InitiatedBy = tierUpdateEvent.InitiatedBy,
                    CustomerId = tierUpdateEvent.CustomerId,
                    NewTier = tierUpdateEvent.NewTier,
                    OldTier = tierUpdateEvent.OldTier
                });

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.EventInitiatedMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MemberTierUpdateEventHandler failed due to Exception", ex.StackTrace);
                throw;
            }
        }
    }
}