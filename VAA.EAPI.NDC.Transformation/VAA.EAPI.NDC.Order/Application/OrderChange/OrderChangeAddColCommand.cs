using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public class OrderChangeAddColCommand : IRequest<Response<IATA_OrderViewRS>>
    {
        public IATA_OrderChangeRQ OrderChangeRQ { get; set; }
        public string SubscriptionKey { get; set; }
    }
}
