using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS
{
    public class OrderDataMSQuery : IRequest<OrderRS>
    {
        public string OrderID { get; set; }

        public static explicit operator OrderDataMSQuery(string OrderID) => new() { OrderID = OrderID };
    }
}
