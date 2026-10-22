using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class SSADelinkOnTierUpdateCommandHandler : IRequestHandler<SSADelinkOnTierUpdateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SSADelinkOnTierUpdateCommandHandler> _logger;

        public SSADelinkOnTierUpdateCommandHandler(IMediator mediator,
            ILogger<SSADelinkOnTierUpdateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(SSADelinkOnTierUpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.OldTier != Constants.Application.FCMemberGold)
                {
                    _logger.LogError($"SSA Delink on MemberTierUpdateEvent - Invalid tier details for SSA delinking");
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                if (request.NewTier != Constants.Application.FCMemberSilver &&
                    request.NewTier != Constants.Application.FCMemberRed)
                {
                    _logger.LogError($"SSA Delink on MemberTierUpdateEvent - Invalid tier details for SSA delinking");
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                await _mediator.Send(new SSADelinkOnMemberStatusUpdateCommand()
                {
                    InitiatedBy = request.InitiatedBy,
                    HeadCustomerId = request.CustomerId,
                });

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.EventInitiatedMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SSA Delink Account on Member Status Update Event Command Handler failed due to Exception", ex.StackTrace);
                throw;
            }
        }
    }
}