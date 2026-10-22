using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders
{
    public class B2CGraphSvcQuery : IRequest<KeyValuePair<bool, string>>
    {
        public string CustomerId { get; set; }
        public B2CGraphSvcQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}
