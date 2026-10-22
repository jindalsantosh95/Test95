using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner
{
    public class PatchBannerCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<BannerResponse>> logger) : TransactionBaseCommandHandler<BannerResponse>(mediator, logger), IRequestHandler<PatchBannerCommand, Result<Response<BannerResponse>>>
    {
        public async Task<Result<Response<BannerResponse>>> Handle(PatchBannerCommand request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
