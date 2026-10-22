using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;

namespace VAA.EAPI.NDC.Order.Application.OrderCreate
{
    public class OrderCreateCommand : IRequest<Result<Response<IATA_OrderViewRS>>>
    {
        public IATA_OrderCreateRQ OrderCreateRQ { get; set; }
        public string SubscriptionKey { get; set; }
    }
}
