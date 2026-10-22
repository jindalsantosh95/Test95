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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Audit;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class SupplementarySilverDelinkingCommandHandler : IRequestHandler<SupplementarySilverDelinkingCommand, Result<Response<SupplementarySilverDelinkingResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly IValidator<SupplementarySilverDelinkingCommand> _validator;

        public SupplementarySilverDelinkingCommandHandler(IMediator mediator,
           IValidator<SupplementarySilverDelinkingCommand> validator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        private async Task UpdateSSAChildFCMDoc(string childAccountCustomerId,
            string initiatedBy,
            DateTime currentTime)
        {
            FlyingClubMember memberDoc = new FlyingClubMember()
            {
                Id = childAccountCustomerId,
                CustomerId = childAccountCustomerId,
                LastUpdatedBy = initiatedBy,
                LastUpdatedOn = currentTime,
                SSAMemberNumber = string.Empty
            };

            FlyingClubMemberPatchEvent fcMemberPatchEvent = new FlyingClubMemberPatchEvent()
            {
                FlyingClubMemberDocument = memberDoc,
                CustomerId = childAccountCustomerId
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

        public async Task<Result<Response<SupplementarySilverDelinkingResponse>>> Handle(SupplementarySilverDelinkingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var commandValidator = await _validator.ValidateAsync(request);
                if (!commandValidator.IsValid)
                {
                    return Result.Failure<Response<SupplementarySilverDelinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(commandValidator.Errors.FirstOrDefault().ToString()), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                if (request.HeadAccount.CustomerId == request.ChildAccount.CustomerId)
                {

                    return Result.Failure<Response<SupplementarySilverDelinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SameHoHAccountAndChildAccount), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                await UpdateSSAChildFCMDoc(request.ChildAccount.CustomerId,
                    request.InitiatedBy,
                    DateTime.UtcNow);

                DateTime audtEventTime = DateTime.UtcNow;
                LoyaltyGenericAudit SupplementarySilverHeadAuditEvent = new LoyaltyGenericAudit()
                {
                    CustomerId = request.HeadAccount.CustomerId,
                    SourceIdentifier = string.Empty,
                    AuditCategory = Constants.Application.LoyaltyGenericAuditSSAAuditCategory,
                    AuditSubCategory = Constants.Application.LoyaltyGenericAuditSSAAuditSubCategoryDelinking,
                    RelationshipType = Constants.Application.LoyaltyGenericAuditSSAAuditRelationshipTypeSuppSilver,
                    AuditData = request.ChildAccount.MemberId,
                    DataUpdatedBy = request.InitiatedBy,
                    DataUpdatedOn = audtEventTime,
                    CreatedBy = request.InitiatedBy,
                    CreatedOn = audtEventTime,
                };

                await _mediator.Send((SSAAuditCreationEventCommand)SupplementarySilverHeadAuditEvent, CancellationToken.None);

                LoyaltyGenericAudit SupplementarySilverChildAuditEvent = new LoyaltyGenericAudit()
                {
                    CustomerId = request.ChildAccount.CustomerId,
                    SourceIdentifier = string.Empty,
                    AuditCategory = Constants.Application.LoyaltyGenericAuditSSAAuditCategory,
                    AuditSubCategory = Constants.Application.LoyaltyGenericAuditSSAAuditSubCategoryDelinking,
                    RelationshipType = Constants.Application.LoyaltyGenericAuditSSAAuditRelationshipTypeLinkedGold,
                    AuditData = request.HeadAccount.MemberId,
                    DataUpdatedBy = request.InitiatedBy,
                    DataUpdatedOn = audtEventTime,
                    CreatedBy = request.InitiatedBy,
                    CreatedOn = audtEventTime,
                };

                await _mediator.Send((SSAAuditCreationEventCommand)SupplementarySilverChildAuditEvent, CancellationToken.None);

                return new Response<SupplementarySilverDelinkingResponse>
                {
                    Status = Status.OK,
                    Value = new SupplementarySilverDelinkingResponse() { Message = Constants.Application.HoHLinkingInitiatedMessage }
                };
            }
            catch (Exception ex)
            {
                return Result.Failure<Response<SupplementarySilverDelinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}