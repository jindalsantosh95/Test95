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
    public class DeleteHouseHoldInviteCommandHandler : IRequestHandler<DeleteHouseHoldInviteCommand, Result<Response<string>>>
    {

        private readonly IMediator _mediator;
        private readonly ILogger<DeleteHouseHoldInviteCommandHandler> _logger;

        public DeleteHouseHoldInviteCommandHandler(IMediator mediator, ILogger<DeleteHouseHoldInviteCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(DeleteHouseHoldInviteCommand request, CancellationToken cancellationToken)
        {
            if (request == null || request.DeleteHouseHoldInvite == null)
                throw new ArgumentNullException(nameof(request));

            CloudEvent cloudEvent = null;
            try
            {
                cloudEvent = new CloudEvent()
                {
                    Data = new DeleteInviteEventRequest()
                    {
                        InviteeMemberNumber = request.DeleteHouseHoldInvite.InviteeMemberId,
                    },
                    Type = Constants.Application.DeleteInvite,
                    Source = Constants.Application.PartnerMembershipEventTopicName,
                    Subject = Constants.Application.DeleteInvite,
                    Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                    Specversion = Constants.Application.SpecVersion
                };

                await _mediator.Send(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent,
                    Subject = cloudEvent.Subject
                }, cancellationToken);

                await _mediator.Publish(cloudEvent);

                return Result.Success(new Response<string> { Status = Status.OK, Value = "Successfully deleted household invite document." });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while publishing Delete Pending Invite event");

                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                return Result.Failure<Response<string>>(Convert.ToString(ex.Message));
            }
        }
    }
}
