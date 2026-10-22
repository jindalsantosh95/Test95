using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class SupplementarySilverAccountDetailsQueryHandler : IRequestHandler<SupplementarySilverAccountDetailsQuery, Result<Response<SupplementarySilverAccountResponse>>>
    {
        private readonly IMediator _mediator;

        public SupplementarySilverAccountDetailsQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<SupplementarySilverAccountResponse>>> Handle(SupplementarySilverAccountDetailsQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerId))
            {
                return Result.Failure<Response<SupplementarySilverAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }

            Result<Response<FlyingClubMember>> response = await _mediator.Send(request: new FlyingClubMemberDocumentServiceQuery { CustomerId = request.CustomerId });
            FlyingClubMember flyingClubMemberDoc = response.Value.Value;

            if (flyingClubMemberDoc == null)
            {
                return Result.Failure<Response<SupplementarySilverAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }

            if (!string.IsNullOrWhiteSpace(flyingClubMemberDoc.TierCode) &&
                (flyingClubMemberDoc.TierCode == Constants.Application.FCMemberRed ||
                flyingClubMemberDoc.TierCode == Constants.Application.FCMemberSilver || !string.IsNullOrEmpty(flyingClubMemberDoc.SSAMemberNumber)))
            {
                return new Response<SupplementarySilverAccountResponse>
                {
                    Status = Status.OK,
                    Value = new SupplementarySilverAccountResponse()
                    {
                        ssaHeadMember = new SupplementarySilverMemberAccount()
                        {
                            MemberNumber = string.IsNullOrEmpty(flyingClubMemberDoc.SSAMemberNumber) ? null : flyingClubMemberDoc.SSAMemberNumber,
                            RelationShipType = string.IsNullOrWhiteSpace(flyingClubMemberDoc.SSAMemberNumber) ? null : Constants.Application.SSAMemberRelationShipTypeCodeLinkedGold
                        }
                    }
                };
            }

            if (string.IsNullOrWhiteSpace(flyingClubMemberDoc.TierCode) ||
                flyingClubMemberDoc.TierCode != Constants.Application.FCMemberGold)
            {
                return Result.Failure<Response<SupplementarySilverAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }

            response = await _mediator.Send(request: new FCMDocBySSMemberNumberServiceQuery { SSAMemberNumber = flyingClubMemberDoc.MemberNumber });
            FlyingClubMember childFCMDoc = response.Value.Value;

            return new Response<SupplementarySilverAccountResponse>
            {
                Status = Status.OK,
                Value = new SupplementarySilverAccountResponse()
                {
                    ssaChildMember = new SupplementarySilverMemberAccount()
                    {
                        MemberNumber = childFCMDoc?.MemberNumber,
                        RelationShipType = (childFCMDoc == null) ? null : Constants.Application.SSAMemberRelationShipTypeCodeSupplementarySilver
                    }
                }
            };
        }
    }
}