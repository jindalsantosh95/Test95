using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Model.OrderList
{
    public class CosmosQueryParameters
    {
        public string Query { get; set; }
        public List<KeyValuePair<string, object>> Parameters { get; set; }
    }
}
