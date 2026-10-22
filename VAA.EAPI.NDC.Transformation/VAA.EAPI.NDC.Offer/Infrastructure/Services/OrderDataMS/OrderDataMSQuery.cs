using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderDataMS
{
    public class OrderDataMSQuery : IRequest<OrderDocument>
    {
        public string OrderID { get; set; }

        public static explicit operator OrderDataMSQuery(string orderID) => new() { OrderID = orderID };
    }
}
