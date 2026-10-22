namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    public class OrderVoucherRedemption
    {
        public string VoucherId { get; set; }
        public long PointsUsed { get; set; }
    }
}
