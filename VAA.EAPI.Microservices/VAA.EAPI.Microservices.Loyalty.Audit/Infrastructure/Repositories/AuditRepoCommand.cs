using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories
{
    public class AuditRepoCommand : IRequest<Result<Unit>>
    {
        public CosmosDocumentEvent Event { get; }
        public AuditRepoCommand(CosmosDocumentEvent cosmosEvent)
        {
            Event = cosmosEvent;
        }

    }
    public class AuditRepoCommandHandler : IRequestHandler<AuditRepoCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;

        public AuditRepoCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Unit>> Handle(AuditRepoCommand request, CancellationToken cancellationToken)
        {
            CosmosModifyRepoCommand command = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.AuditDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.AuditDBName),
                CollectionName = ConfigReader.Get(Constants.Environment.AuditDBCollection),
                Event = request.Event
            };

            await _mediator.Send(command, cancellationToken);
            return Unit.Value;
        }
    }
}
