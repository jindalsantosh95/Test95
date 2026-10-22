using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class RefundAPIRepoCommand : IRequest<bool>
    {
        public VoidTransactionRequest VoidTransactionRequest { get; set; }
    }
}
