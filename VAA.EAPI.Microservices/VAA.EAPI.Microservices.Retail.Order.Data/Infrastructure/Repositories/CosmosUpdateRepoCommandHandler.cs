using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class CosmosUpdateRepoCommandHandler : IRequestHandler<CosmosUpdateRepoCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;

        public CosmosUpdateRepoCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Unit>> Handle(CosmosUpdateRepoCommand request, CancellationToken cancellationToken)
        {
            CosmosModifyRepoCommand command = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.DatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.OrderCollection),
                Event = request.Event,
            };

            await _mediator.Send(command, cancellationToken);

            return Unit.Value;
        }
    }
}
