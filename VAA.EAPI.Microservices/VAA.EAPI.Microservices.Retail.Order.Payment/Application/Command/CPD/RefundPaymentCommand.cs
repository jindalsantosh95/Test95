using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public class RefundPaymentCommand : IRequest<Result<Response<Domain.CPD.PaymentResponse>>>
    {
        public RefundPaymentRequest RefundPaymentRequest { get; set; }
    }
}
