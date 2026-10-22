using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Application.Command
{
    public interface IOrderRefundCommand
    {
        Task<Result<Response<PaymentResponse>>> Handle(AuthorizePaymentRequest authorizePaymentRequest, CancellationToken cancellationToken);
    }
}
