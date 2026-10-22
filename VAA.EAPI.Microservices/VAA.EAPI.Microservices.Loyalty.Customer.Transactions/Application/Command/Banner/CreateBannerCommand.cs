using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner
{
   public class CreateBannerCommand : TransactionBaseCommand, IRequest<Result<Response<string>>>
    {
        public CreateBannerRequest BannerRequest { get; set; }

        public CreateBannerCommand(CreateBannerRequest bannerRequest)
        {
            BannerRequest = bannerRequest;
        }
    }
}
