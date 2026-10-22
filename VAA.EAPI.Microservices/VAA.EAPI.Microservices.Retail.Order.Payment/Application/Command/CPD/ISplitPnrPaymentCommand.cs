using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public interface ISplitPnrPaymentCommand
    {
        Task<Result<Response<IEnumerable<PaymentResponse>>>>Handle(SplitPnrPaymentRequest request, CancellationToken cancellationToken);
    }
}
