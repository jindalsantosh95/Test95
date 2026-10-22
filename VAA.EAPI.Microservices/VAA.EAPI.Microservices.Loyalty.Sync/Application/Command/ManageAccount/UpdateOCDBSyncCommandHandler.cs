using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Command.ManageAccount
{

    public class UpdateOCDBSyncCommandHandler : IRequestHandler<UpdateOCDBSyncCommand, Result<bool>>
    {
        private readonly IMediator _mediator;

        public UpdateOCDBSyncCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<bool>> Handle(UpdateOCDBSyncCommand request, CancellationToken cancellationToken)
        {
            if (request.Data == null) throw new ArgumentNullException(nameof(request));

            Validations.ValidatePayload.Validate(request.Data);
            return (await _mediator.Send(new UpdateOCDBSyncRepoCommand(request.Data), cancellationToken)).Value;
        }
    }
}
