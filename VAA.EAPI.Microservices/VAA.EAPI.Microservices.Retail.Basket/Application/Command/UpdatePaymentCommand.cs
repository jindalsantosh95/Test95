using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdatePaymentCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public IEnumerable<PaymentSession> PaymentSessions { get; set; }
    }
}
