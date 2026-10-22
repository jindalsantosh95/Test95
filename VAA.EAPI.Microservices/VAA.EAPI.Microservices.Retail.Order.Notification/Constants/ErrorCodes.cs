namespace VAA.EAPI.Microservices.Retail.Order.Notification.Constants
{
    public static class ErrorCodes
    {
        public static readonly string invalidOrderReference = "ON-001";
        public static readonly string invalidChannel = "ON-002";
        public static readonly string invalidDateTimeFormat = "G010";
        public static readonly string invalidFulfillmentDetails = "ON-003";
        public static readonly string invalidIdType = "ON-003";
        public static readonly string invalidFulfillmentId = "ON-004";
        public static readonly string ServiceBusError = "G034";
    }
}
