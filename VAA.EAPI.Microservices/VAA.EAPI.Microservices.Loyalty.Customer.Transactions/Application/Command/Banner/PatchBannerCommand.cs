using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner
{
    public class PatchBannerCommand : BannerCommandBase, IRequest<Result<Response<BannerResponse>>>
    {
        public PatchBannerCommand()
        {
            EventType = TransactionCommandType.UpdateBanner;
        }
    }
}
