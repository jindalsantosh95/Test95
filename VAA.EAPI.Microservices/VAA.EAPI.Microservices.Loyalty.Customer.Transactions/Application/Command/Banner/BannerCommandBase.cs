using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner
{
    public class BannerCommandBase : TransactionBaseCommand
    {
        public BannerDetail Banner { get; set; }

        public BannerCommandBase()
        {
            DocType = Constants.Application.BannerDocType;
        }

        public static async Task<BannerResponse> GetBanner(IMediator mediator, string customerId, CancellationToken cancellationToken)
        {
            return (await mediator.Send(new BannerQuery()
            {
                CustomerId = customerId,
            }, cancellationToken)).Value.Value;
        }
    }
}
