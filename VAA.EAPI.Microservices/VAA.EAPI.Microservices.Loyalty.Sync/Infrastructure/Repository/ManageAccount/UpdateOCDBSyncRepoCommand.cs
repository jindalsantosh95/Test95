using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.ManageAccount
{
    public class UpdateOCDBSyncRepoCommand : IRequest<Result<bool>>
    {
        public LoyaltySyncRequestData UpdateOCDBSyncQuery { get; set; }

        public UpdateOCDBSyncRepoCommand(LoyaltySyncRequestData updateOCDBSyncQuery)
        {
            UpdateOCDBSyncQuery = updateOCDBSyncQuery;
        }
    }
}
