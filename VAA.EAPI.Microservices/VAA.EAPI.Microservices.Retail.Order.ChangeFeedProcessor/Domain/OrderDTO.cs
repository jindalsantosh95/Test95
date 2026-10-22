using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public class OrderDTO : CosmosDocument
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public double TS { get; set; }
        public IEnumerable<CustomerDetail> CustomerDetails { get; set; }
        public IEnumerable<SegmentDetail> SegmentDetails { get; set; }

        public static explicit operator OrderDTO(OrderDocument order)
        {
            return new()
            {
                Id = order.Id,
                OrderId = order.OrderId ?? string.Empty,
                CustomerDetails = order.CustomerDetails,
                SegmentDetails = order.SegmentDetails,
                TS = order.TS
            };
        }
    }
}
