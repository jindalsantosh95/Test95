using CSharpFunctionalExtensions;
using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.Loyalty.SyncRepository;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.Loyalty.SyncService
{
    public class LoyaltySyncServiceCommand : IRequest<Result<AddResponseType>>
    {
        public LoyaltySyncRepoCommand LoyaltySyncRepoQuery { get; set; }

        public LoyaltySyncServiceCommand(LoyaltySyncRepoCommand loyaltySyncRepoQuery)
        {
            LoyaltySyncRepoQuery = loyaltySyncRepoQuery;
        }
    }
}
