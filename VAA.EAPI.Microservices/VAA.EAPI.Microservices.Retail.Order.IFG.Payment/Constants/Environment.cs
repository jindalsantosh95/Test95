namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Constants
{
    public class Environment
    {
        #region IFG

        public static readonly string PaymentCollection = "PaymentCollection";
        public static readonly string PaymentDatabaseName = "PaymentDatabaseName";
        public static readonly string PaymentDBEndpoint = "PaymentDBEndpoint";
        public static readonly string IFGUsername = "IFGUsername";
        public static readonly string IFGPassword = "IFGPassword";
        public static readonly string IFGAuthorizeRequestUrl = "IFGAuthorizeRequestUrl";
        public static readonly string IFGOrderRequestUrl = "IFGOrderRequestUrl";
        public static readonly string IFGExchangeRequestUrl = "IFGExchangeRequestUrl";
        public static readonly string IFGRefundRequestUrl = "IFGRefundRequestUrl";
        public static readonly string IFGAuthorizeCancelRequestUrl = "IFGAuthorizeCancelRequestUrl";
        public static readonly string OrderApiRetryCount = "OrderApiRetryCount";
        public static readonly string OrderRetryDelayMilliseconds = "OrderRetryDelayMilliseconds";
        public static readonly string IFGEnvironment = "IFGEnvironment";
        public static readonly string IfgAdditionalFieldsEnabled = "IfgAdditionalFieldsEnabled";
        #endregion
    }
}
