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
    public class AwardDeleteCommand : AwardCommandBase, IRequest<Result<Response<AwardCommandResponse>>>
    {
        public AwardDeleteCommand()
        {
            EventType = TransactionCommandType.DeleteAward;

        }
    }
    public class AwardDeleteCommandHandler : TransactionBaseCommandHandler<AwardCommandResponse>, IRequestHandler<AwardDeleteCommand, Result<Response<AwardCommandResponse>>>
    {
        public AwardDeleteCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<AwardCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public async Task<Result<Response<AwardCommandResponse>>> Handle(AwardDeleteCommand request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        //protected override async Task<TransactionBaseCommand> ValidateRequest(TransactionBaseCommand command, CancellationToken cancellationToken)
        //{
        //    // Validate request starts with the basic validation
        //    var validatedCommand = (AwardCommandBase)command;


        //    if (!new AwardDeleteValidator().Validate(validatedCommand).IsValid)
        //        throw new RequestValidationException("Award Delete Request contains invalid data or missing mandatory fields.");

        //    // Get a list of awards
        //    IEnumerable<Award> currentAwards = await AwardCommandBase.GetListOfAwards(_mediator, command.CustomerId, cancellationToken);
        //    if (!currentAwards.Any(a => a.Id == validatedCommand.Award.Id))
        //        throw new RequestValidationException("Unable to find award record for Award Delete Request.");

        //    return validatedCommand;
        //}

    }
}
