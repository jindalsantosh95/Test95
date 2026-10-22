namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public sealed class OrderRetrieveResponseMapOptions
    {
        public bool IsHoldBooking { get; set; }
        public bool IsCashPayment { get; set; }
        public bool IsEasyPay { get; set; }
        public string ApprovalCode { get; set; }
        public bool IsDisruptedOrderRetrieve { get; set; }
    }
}
