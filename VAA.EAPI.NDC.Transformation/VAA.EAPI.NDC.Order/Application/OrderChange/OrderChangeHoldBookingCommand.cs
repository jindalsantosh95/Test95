using MediatR;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class OrderChangeHoldBookingCommand : IRequest<Response<IATA_OrderViewRS>>
    {
        public IATA_OrderChangeRQ OrderChangeRQ { get; set; }
        public string SubscriptionKey { get; set; }
    }
}
