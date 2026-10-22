using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository
{
    public class CustomerSearchUpdateRepoCommandHandler : IRequestHandler<CustomerSearchUpdateRepoCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;

        public CustomerSearchUpdateRepoCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Unit>> Handle(CustomerSearchUpdateRepoCommand request, CancellationToken cancellationToken)
        {
            CosmosModifyRepoCommand command = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SCVCustSearchDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.SCVCustSearchDBName),
                CollectionName = ConfigReader.Get(Constants.Environment.SCVCustSearchDBCollection),
                Event = request.Event,
            };

            await _mediator.Send(command, cancellationToken);

            return Unit.Value;
        }
    }
}
