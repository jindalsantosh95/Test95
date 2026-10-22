using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MembershipChange;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MembershipChange
{
    public class MembershipChangeCreateCommand : MembershipChangeCommandBase, IRequest<Result<Response<MembershipChangeCommandResponse>>>
    {
        public MembershipChangeCreateCommand() : base()
        {
            EventType = TransactionCommandType.CreateMembershipChange;
        }
    }

    public class MembershipChangeCreateCommandHandler : TransactionBaseCommandHandler<MembershipChangeCommandResponse>, IRequestHandler<MembershipChangeCreateCommand, Result<Response<MembershipChangeCommandResponse>>>
    {
        public MembershipChangeCreateCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<MembershipChangeCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<MembershipChangeCommandResponse>>> Handle(MembershipChangeCreateCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }
}
