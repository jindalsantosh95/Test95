using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentEventCommand : IRequest<Result<Response<string>>>
    {
        public string Channel { get; set; }
        public string OrderReference { get; set; }
        public List<OrderPaymentRequest> PaymentRequest { get; set; }
    }
}
