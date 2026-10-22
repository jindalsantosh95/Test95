using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.InviteHouseHold
{
    public class InviteHouseHoldCommandHandler : IRequestHandler<InviteHouseHoldCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InviteHouseHoldCommandHandler> _logger;

        public InviteHouseHoldCommandHandler(IMediator mediator, ILogger<InviteHouseHoldCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(InviteHouseHoldCommand request, CancellationToken cancellationToken)
        {
            if (request == null || request.InviteHouseHold == null)
                throw new ArgumentNullException(nameof(request));

            CloudEvent cloudEvent = null;
            try
            {
                cloudEvent = new CloudEvent()
                {
                    Data = (Inviter)request.InviteHouseHold,
                    Type = Constants.Application.PendingInvite,
                    Source = Constants.Application.PartnerMembershipEventTopicName,
                    Subject = Constants.Application.PendingInvite,
                    Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                    Specversion = Constants.Application.SpecVersion
                };

                await _mediator.Send(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent,
                    Subject = cloudEvent.Subject
                });

                await _mediator.Publish(cloudEvent);

                PublishEmailEvent(request.InviteHouseHold, cancellationToken).GetAwaiter();

                return Result.Success(new Response<string> { Status = Status.OK, Value = "Successfully sent household invitation" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while publishing Pending Invite event");
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                return Result.Failure<Response<string>>(Convert.ToString(ex.Message));
            }
        }

        private async Task PublishEmailEvent(InviteHouseHoldCreateRequest request, CancellationToken cancellationToken)
        {
            CloudEvent emailCloudEvent = new()
            {
                Data = new AdobeMemberInviteEmailCommand
                {
                    Email = request.InviteeEmail,
                    AdobeInviteMemberRequest = new AdobeMemberInviteRequest
                    {
                        FirstName = request.InviteeFirstName,
                        LastName = request.InviteeLastName,
                        MembershipNumber = request.InviteeMemberId,
                        FirstNameOfHouseholdLead = request.InviterFirstName,
                        FirstNameOfHouseholdMember = request.InviteeFirstName,
                    }
                },
                Type = Constants.Application.PendingInviteEmail,
                Source = Constants.Application.InviteHouseHoldEmailEventTopicName,
                Subject = Constants.Application.PendingInviteEmail,
                Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };

            await _mediator.Publish(emailCloudEvent, cancellationToken);
        }
    }
}
