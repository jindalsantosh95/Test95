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
    public class AwardPatchCommand : AwardCommandBase, IRequest<Result<Response<AwardCommandResponse>>>
    {
        public AwardPatchCommand()
        {
            EventType = TransactionCommandType.UpdateAward;
        }
    }
    public class AwardPatchCommandHandler : TransactionBaseCommandHandler<AwardCommandResponse>, IRequestHandler<AwardPatchCommand, Result<Response<AwardCommandResponse>>>
    {
        public AwardPatchCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<AwardCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public async Task<Result<Response<AwardCommandResponse>>> Handle(AwardPatchCommand request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        //protected override async Task<TransactionBaseCommand> ValidateRequest(TransactionBaseCommand command, CancellationToken cancellationToken)
        //{
        //    var validatedCommand = (AwardCommandBase)command;

        //    // Validate
        //    if (!new AwardPatchValidator().Validate(validatedCommand).IsValid)
        //        throw new RequestValidationException("Award Update Request contains invalid data or missing mandatory fields.");

        //    // Get a list of awards
        //    IEnumerable<Award> currentAwards = await AwardCommandBase.GetListOfAwards(_mediator, command.CustomerId, cancellationToken);
        //    if (!currentAwards.Any(a => a.Id == validatedCommand.Award.Id))
        //        throw new RequestValidationException("Unable to find award record for Award Update Request.");

        //    // Record who is making update
        //    validatedCommand.Award.LastUpdatedBy = command.Originator;
        //    validatedCommand.Award.LastUpdatedOn = DateTime.Now;

        //    return validatedCommand;
        //}
    }
}
