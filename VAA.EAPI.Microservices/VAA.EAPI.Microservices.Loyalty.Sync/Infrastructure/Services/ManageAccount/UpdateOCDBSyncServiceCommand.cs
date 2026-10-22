using CSharpFunctionalExtensions;
using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.ManageAccount
{
    public class UpdateOCDBSyncServiceCommand : IRequest<Result<UpdateResponseType>>
    {
        public LoyaltySyncRequestData UpdateOCDBSyncRepoQuery { get; set; }

        public UpdateOCDBSyncServiceCommand(LoyaltySyncRequestData updateOCDBSyncRepoQuery)
        {
            UpdateOCDBSyncRepoQuery = updateOCDBSyncRepoQuery;
        }
    }
}
