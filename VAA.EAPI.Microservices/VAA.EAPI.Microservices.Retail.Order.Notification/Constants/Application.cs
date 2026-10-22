namespace VAA.EAPI.Microservices.Retail.Order.Notification.Constants
{
    public static class Application
    {
        public static readonly string EventSpecversion = "1.0";
        public static readonly string OrderNotificationEventSubject = "OrderNotification";
        public static readonly string OrderNotifyDateTimeISOFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public static readonly string OrderNotifyDateFormat = "yyyy-MM-dd";
        public static readonly string EventUtcTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string ChannelPathParam = "channel";
        public static readonly string ChannelConfigFilePath = "Infrastructure\\ChannelConfiguration\\ChannelConfiguration.json";
    }
}
