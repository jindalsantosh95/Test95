using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{

    public class DelinkAccountOnMemberStatusUpdateCommandHandler : IRequestHandler<DelinkAccountOnMemberStatusUpdateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DelinkAccountOnMemberStatusUpdateCommandHandler> _logger;

        public DelinkAccountOnMemberStatusUpdateCommandHandler(IMediator mediator, ILogger<DelinkAccountOnMemberStatusUpdateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(DelinkAccountOnMemberStatusUpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                MemberStatus memberStatus = Json.Deserialize<MemberStatus>(Convert.ToString(request.Data));
                if (memberStatus.NewStatus != "Active" && memberStatus.NewStatus != "Frozen" && memberStatus.NewStatus != "Suspended")
                {
                    //Delink SSA Accounts
                    await _mediator.Send(new SSADelinkOnMemberStatusUpdateCommand()
                    {
                        InitiatedBy = memberStatus.InitiatedBy,
                        HeadCustomerId = memberStatus.CustomerID,
                    });

                    if (memberStatus.GroupID != null && memberStatus.MemberType == Constants.Application.HouseholdGroupTypeCode)
                    {
                        DelinkAccountMemberPatchEvent dlinkAccntMemberPatchEvent = new DelinkAccountMemberPatchEvent()
                        {
                            CustomerID = memberStatus.CustomerID,
                            InitiatedBy = memberStatus.InitiatedBy,
                            GroupID = memberStatus.GroupID
                        };
                        var cloudEvent = DelinkAccountMemberPatchEventToCloudEventMapper.CommandToEventMapper(dlinkAccntMemberPatchEvent);

                        try
                        {
                            await _mediator.Send(new EventRequestInsertCommand
                            {
                                Id = cloudEvent.Id,
                                Type = cloudEvent.Type,
                                Data = cloudEvent,
                                Subject = cloudEvent.Subject
                            });
                            await _mediator.Publish(cloudEvent);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Process failed due to Exception - {ex.Message}");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                            throw;
                        }
                    }
                }

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.DeleteSuccessMessage
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Delink Account on Member Status Update Event Command Handler failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }


    }
}
