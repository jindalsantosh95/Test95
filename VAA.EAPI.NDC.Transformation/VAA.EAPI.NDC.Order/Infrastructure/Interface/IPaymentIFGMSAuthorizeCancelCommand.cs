using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Interface
{
    public interface IPaymentIFGMSAuthorizeCancelCommand
    {
        Task Handle(AuthorizePaymentRequest request, CancellationToken cancellationToken);
    }
}
