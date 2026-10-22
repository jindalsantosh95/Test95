using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query.CPD
{
    public class PaymentDocQuery : IRequest<Response<IEnumerable<PaymentTransactionDocument>>>
    {
        public string OrderId { get; set; }
    }
}
