using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.TargetedMemberships
{
    public class TargetedMembershipDeleteCommand : TargetedMembershipCommandBase, IRequest<Result<Response<TargetedMembershipCommandResponse>>>
    {
        public TargetedMembershipDeleteCommand()
        {
            EventType = TransactionCommandType.DeleteTargetedMembership;

        }
    }
    public class TargetedMembershipDeleteCommandHandler : TransactionBaseCommandHandler<TargetedMembershipCommandResponse>, IRequestHandler<TargetedMembershipDeleteCommand, Result<Response<TargetedMembershipCommandResponse>>>
    {
        public TargetedMembershipDeleteCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<TargetedMembershipCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<TargetedMembershipCommandResponse>>> Handle(TargetedMembershipDeleteCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }
}
