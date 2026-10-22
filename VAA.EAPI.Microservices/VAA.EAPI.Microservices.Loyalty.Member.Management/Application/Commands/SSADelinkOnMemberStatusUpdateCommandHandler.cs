using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class SSADelinkOnMemberStatusUpdateCommandHandler : IRequestHandler<SSADelinkOnMemberStatusUpdateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SSADelinkOnMemberStatusUpdateCommandHandler> _logger;

        public SSADelinkOnMemberStatusUpdateCommandHandler(IMediator mediator,
            ILogger<SSADelinkOnMemberStatusUpdateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(SSADelinkOnMemberStatusUpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InitiatedBy) ||
                    string.IsNullOrWhiteSpace(request.HeadCustomerId))
                {
                    _logger.LogError($"Delink SSA on Member Status Update Event - Invalid request. Attributes 'InitiatedBy' and 'HeadCustomerId' are mandatory");
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                var parentAccountResponse = await _mediator.Send(request: new FlyingClubMemberDocumentServiceQuery { CustomerId = request.HeadCustomerId });
                FlyingClubMember flyingClubMemberDoc = parentAccountResponse.Value.Value;

                if (flyingClubMemberDoc == null)
                {
                    _logger.LogError($"Delink SSA on Member Status Update Event - FlyingclubMemberDocument not found for the customerId - {request.HeadCustomerId}");
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                var childAccountResponse = await _mediator.Send(request: new FCMDocBySSMemberNumberServiceQuery { SSAMemberNumber = flyingClubMemberDoc.MemberNumber });
                FlyingClubMember childFCMDoc = childAccountResponse.Value.Value;

                if (childFCMDoc == null)
                {
                    _logger.LogError($"Delink SSA on Member Status Update Event - Linked account not found for the specified customerId - {flyingClubMemberDoc.MemberNumber}");
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SupplimentarySilverAccountNotFound), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                await _mediator.Send(new SupplementarySilverDelinkingCommand()
                {
                    InitiatedBy = request.InitiatedBy,
                    HeadAccount = new HouseHoldAccount()
                    {
                        CustomerId = request.HeadCustomerId,
                        MemberId = flyingClubMemberDoc.MemberNumber
                    },
                    ChildAccount = new HouseHoldAccount()
                    {
                        CustomerId = childFCMDoc.CustomerId,
                        MemberId = childFCMDoc.MemberNumber
                    }
                });

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.DeleteSuccessMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Delink Account on Member Status Update Event Command Handler failed due to Exception");
                throw;
            }
        }
    }
}