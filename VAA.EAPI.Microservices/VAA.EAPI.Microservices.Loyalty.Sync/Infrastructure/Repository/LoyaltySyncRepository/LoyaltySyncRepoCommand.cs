using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.Loyalty.SyncRepository
{
    public class LoyaltySyncRepoCommand : IRequest<Result<bool>>
    {
        public LoyaltySyncEventProcessCommand LoyaltySyncQuery { get; set; }

        public LoyaltySyncRepoCommand(LoyaltySyncEventProcessCommand loyaltySyncQuery)
        {
            LoyaltySyncQuery = loyaltySyncQuery;
        }
    }
}
