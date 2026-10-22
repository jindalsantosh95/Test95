using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Audit;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class SupplementarySilverAccountLinkCommandHandler : IRequestHandler<SupplementarySilverAccountLinkCommand, Result<Response<SupplementarySilverAccountLinkingResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly IValidator<SupplementarySilverAccountLinkCommand> _validator;

        public SupplementarySilverAccountLinkCommandHandler(IMediator mediator,
            IValidator<SupplementarySilverAccountLinkCommand> validator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        private async Task UpdateChildFCMDoc(string customerId,
            string ssaMemberNumber,
            string tierCode,
            string initiatedBy)
        {
            FlyingClubMember memberDoc = new FlyingClubMember()
            {
                Id = customerId,
                CustomerId = customerId,
                LastUpdatedBy = initiatedBy,
                SSAMemberNumber = ssaMemberNumber,
                LastUpdatedOn = DateTime.UtcNow
            };

            FlyingClubMemberPatchEvent fcMemberPatchEvent = new FlyingClubMemberPatchEvent()
            {
                FlyingClubMemberDocument = memberDoc,
                CustomerId = customerId,
                HeadTierCode = tierCode
            };

            var cloudEvent = FlyingClubMemberPatchEventToCloudEventMapper.CommandToEventMapper(fcMemberPatchEvent);

            await _mediator.Send(new EventRequestInsertCommand
            {
                Id = cloudEvent.Id,
                Type = cloudEvent.Type,
                Data = cloudEvent,
                Subject = cloudEvent.Subject
            });

            try
            {
                await _mediator.Publish(cloudEvent);
            }
            catch (Exception ex)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }

        public async Task<Result<Response<SupplementarySilverAccountLinkingResponse>>> Handle(SupplementarySilverAccountLinkCommand request, CancellationToken cancellationToken)
        {
            var commandValidator = await _validator.ValidateAsync(request);

            if (!commandValidator.IsValid)
            {
                return Result.Failure<Response<SupplementarySilverAccountLinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(commandValidator.Errors.FirstOrDefault().ToString()), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }

            var headCustomerResponse = await _mediator.Send(request: new FlyingClubMemberDocumentServiceQuery { CustomerId = request.HeadCustomerId });
            FlyingClubMember headFCMDoc = headCustomerResponse.Value.Value;

            if (headFCMDoc != null)
            {
                if (headFCMDoc.TierCode != Constants.Application.FCMemberGold)
                {
                    return Result.Failure<Response<SupplementarySilverAccountLinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SSAInvalidParentTierCode), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                if (headFCMDoc.Status != Constants.Application.FCMemberActiveStatus)
                {
                    return Result.Failure<Response<SupplementarySilverAccountLinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SSAInvalidParentStatus), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                var ssaFCMDocResponse = await _mediator.Send(request: new FCMDocBySSMemberNumberServiceQuery { SSAMemberNumber = request.HeadMemberNumber });
                FlyingClubMember ssaFCMDoc = ssaFCMDocResponse.Value.Value;

                if (ssaFCMDoc != null)
                {
                    return Result.Failure<Response<SupplementarySilverAccountLinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SSAlinkedGoldParentAccount), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                var childCustomerResponse = await _mediator.Send(request: new FlyingClubMemberDocumentServiceQuery { CustomerId = request.ChildCustomerId });
                FlyingClubMember childFCMDoc = childCustomerResponse.Value.Value;

                if (childFCMDoc != null)
                {
                    if (childFCMDoc.TierCode != Constants.Application.FCMemberRed &&
                        childFCMDoc.TierCode != Constants.Application.FCMemberSilver)
                    {
                        return Result.Failure<Response<SupplementarySilverAccountLinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SSAInvalidChildTierCode), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }

                    if (childFCMDoc.Status != Constants.Application.FCMemberActiveStatus &&
                        childFCMDoc.Status != Constants.Application.FCMemberFrozenStatus &&
                        childFCMDoc.Status != Constants.Application.FCMemberActivePendingStatus &&
                        childFCMDoc.Status != Constants.Application.FCMemberActivePendStatus &&
                        childFCMDoc.Status != Constants.Application.FCMemberExpiredStatus)
                    {
                        return Result.Failure<Response<SupplementarySilverAccountLinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SSAInvalidChildStatus), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }
                }
                else
                {
                    return Result.Failure<Response<SupplementarySilverAccountLinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                await UpdateChildFCMDoc(request.ChildCustomerId,
                    request.HeadMemberNumber,
                    headFCMDoc.TierCode,
                    request.InitiatedBy);

                DateTime audtEventTime = DateTime.UtcNow;
                LoyaltyGenericAudit SupplementarySilverHeadAuditEvent = new LoyaltyGenericAudit()
                {
                    CustomerId = request.HeadCustomerId,
                    SourceIdentifier = string.Empty,
                    AuditCategory = Constants.Application.LoyaltyGenericAuditSSAAuditCategory,
                    AuditSubCategory = Constants.Application.LoyaltyGenericAuditSSAAuditSubCategoryLinking,
                    RelationshipType = Constants.Application.LoyaltyGenericAuditSSAAuditRelationshipTypeSuppSilver,
                    AuditData = childFCMDoc.MemberNumber,
                    DataUpdatedBy = request.InitiatedBy,
                    DataUpdatedOn = audtEventTime,
                    CreatedBy = request.InitiatedBy,
                    CreatedOn = audtEventTime,
                };

                await _mediator.Send((SSAAuditCreationEventCommand)SupplementarySilverHeadAuditEvent, CancellationToken.None);

                LoyaltyGenericAudit SupplementarySilverChildAuditEvent = new LoyaltyGenericAudit()
                {
                    CustomerId = request.ChildCustomerId,
                    SourceIdentifier = string.Empty,
                    AuditCategory = Constants.Application.LoyaltyGenericAuditSSAAuditCategory,
                    AuditSubCategory = Constants.Application.LoyaltyGenericAuditSSAAuditSubCategoryLinking,
                    RelationshipType = Constants.Application.LoyaltyGenericAuditSSAAuditRelationshipTypeLinkedGold,
                    AuditData = headFCMDoc.MemberNumber,
                    DataUpdatedBy = request.InitiatedBy,
                    DataUpdatedOn = audtEventTime,
                    CreatedBy = request.InitiatedBy,
                    CreatedOn = audtEventTime,
                };

                await _mediator.Send((SSAAuditCreationEventCommand)SupplementarySilverChildAuditEvent, CancellationToken.None);

                return new Response<SupplementarySilverAccountLinkingResponse>
                {
                    Status = Status.OK,
                    Value = new SupplementarySilverAccountLinkingResponse() { Message = Constants.Application.RequestSuccessfullyInitiatedMessage }
                };
            }
            else
            {
                return Result.Failure<Response<SupplementarySilverAccountLinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}