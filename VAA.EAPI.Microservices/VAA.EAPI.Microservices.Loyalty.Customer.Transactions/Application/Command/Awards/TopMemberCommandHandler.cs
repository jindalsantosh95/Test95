using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Awards
{
    public class TopMemberCommand : AwardCommandBase, IRequest<Result<Response<AwardCommandResponse>>>
    {
        public TopMemberCommand()
        {
            EventType = TransactionCommandType.AssignTopMember;
        }
    }
    public class TopMemberCommandHandler : TransactionBaseCommandHandler<AwardCommandResponse>, IRequestHandler<TopMemberCommand, Result<Response<AwardCommandResponse>>>
    {
        public TopMemberCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<AwardCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public async Task<Result<Response<AwardCommandResponse>>> Handle(TopMemberCommand request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
