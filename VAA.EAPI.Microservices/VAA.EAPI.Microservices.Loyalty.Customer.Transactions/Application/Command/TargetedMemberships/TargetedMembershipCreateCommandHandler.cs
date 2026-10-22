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
    public class TargetedMembershipCreateCommand : TargetedMembershipCommandBase, IRequest<Result<Response<TargetedMembershipCommandResponse>>>
    {
        public TargetedMembershipCreateCommand() : base()
        {
            EventType = TransactionCommandType.CreateTargetedMembership;
        }
    }
    public class TargetedMembershipCreateCommandHandler : TransactionBaseCommandHandler<TargetedMembershipCommandResponse>, IRequestHandler<TargetedMembershipCreateCommand, Result<Response<TargetedMembershipCommandResponse>>>
    {
        public TargetedMembershipCreateCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<TargetedMembershipCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<TargetedMembershipCommandResponse>>> Handle(TargetedMembershipCreateCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }
}
