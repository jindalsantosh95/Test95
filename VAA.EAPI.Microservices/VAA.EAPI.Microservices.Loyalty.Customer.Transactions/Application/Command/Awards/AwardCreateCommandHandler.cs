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
    public class AwardCreateCommand : AwardCommandBase, IRequest<Result<Response<AwardCommandResponse>>>
    {
        public AwardCreateCommand()
        {
            EventType = TransactionCommandType.CreateAward;
        }
    }
    public class AwardCreateCommandHandler : TransactionBaseCommandHandler<AwardCommandResponse>, IRequestHandler<AwardCreateCommand, Result<Response<AwardCommandResponse>>>
    {
        public AwardCreateCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<AwardCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public async Task<Result<Response<AwardCommandResponse>>> Handle(AwardCreateCommand request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
