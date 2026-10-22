using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class PaymentDocRepoQuery : IRequest<IEnumerable<object>>
    {
        public string OrderId { get; set; }
        public string Query { get; set; }
    }
}
