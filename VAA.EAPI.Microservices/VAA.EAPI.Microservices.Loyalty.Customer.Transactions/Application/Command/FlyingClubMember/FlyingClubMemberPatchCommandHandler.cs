using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember
{
    public class FlyingClubMemberPatchCommand : FlyingClubMemberCommandBase, IRequest<Result<Response<FlyingClubMemberCommandResponse>>>
    {
        public FlyingClubMemberPatchCommand()
        {
            EventType = TransactionCommandType.UpdateFlyingClubMemberEvent;
        }
    }

    public class FlyingClubMemberPatchCommandHandler : TransactionBaseCommandHandler<FlyingClubMemberCommandResponse>, IRequestHandler<FlyingClubMemberPatchCommand, Result<Response<FlyingClubMemberCommandResponse>>>
    {
        public FlyingClubMemberPatchCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<FlyingClubMemberCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public async Task<Result<Response<FlyingClubMemberCommandResponse>>> Handle(FlyingClubMemberPatchCommand request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
