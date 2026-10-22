using MediatR;
using System.Collections.Generic;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS
{
    public class PaymentMSRetrieveQuery : IRequest<IEnumerable<PaymentTransactionDocument>>
    {
        public string OrderID { get; set; }
    }
}
