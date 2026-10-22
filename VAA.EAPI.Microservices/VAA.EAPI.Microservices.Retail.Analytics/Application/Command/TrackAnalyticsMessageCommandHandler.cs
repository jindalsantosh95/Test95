using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Analytics.Application.Command
{
    public sealed class TrackAnalyticsMessageCommandHandler : IRequestHandler<TrackAnalyticsMessageCommand, bool>
    {
        private readonly IMediator _mediator;

        public TrackAnalyticsMessageCommandHandler(IMediator mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        public async Task<bool> Handle(TrackAnalyticsMessageCommand request, CancellationToken cancellationToken)
        {
            CosmosModifyServiceCommand cosmosModifyServiceCommand = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.CosmosDatabaseEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.CosmosDatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.CosmosCollectionName),
                Event = new CosmosDocumentEvent()
                {
                    Document = request,
                    ModificationType = Infrastructure.Entities.Enums.CosmosModificationType.Create
                }
            };

            await _mediator.Send(cosmosModifyServiceCommand, cancellationToken);

            return true;
        }
    }


}
