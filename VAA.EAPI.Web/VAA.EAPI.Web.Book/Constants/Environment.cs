namespace VAA.EAPI.Web.Book.Constants
{
    public class Environment
    {
        public static readonly string RetrieveBasketPqId = "GraphQL:RetrieveBasketPqId";
        public static readonly string OrderConfirmationMockResponse = "OrderConfirmationMockResponse";
        public static readonly string OrderConfirmationMockResponseFromCache = "OrderConfirmationMockResponseFromCache";

        public static string BasketLink = "BasketLink";
        public static string ContactLink = "ContactLink";
        public static string TermsLink = "TermsLink";
        public static string PassengerLink = "PassengerLink";
        public static string PaymentLink = "PaymentLink";
        public static string BookCreateOrderPqId = "BookCreateOrderPqId";
        public static string OrderResponsePqId = "OrderResponsePqId";

        //Payment
        public static readonly string PaymentRefIdRequestMockResponse = "PaymentRefIdRequestMockResponse";
        public static string OrderPaymentRefIdPqId = "OrderPaymentRefIdPqId";
        public static string OrderPaymentStatusPqId = "OrderPaymentStatusPqId";
        public static string BffPaymentLink = "BffPaymentLink";
        public static string BffPaymentAmountLink = "BffPaymentAmountLink";
        public static string BffPaymentConfirmationLink = "BffPaymentConfirmationLink";
        public static string PaymentDetailsResponsePqId = "PaymentDetailsResponsePqId";

        public static readonly string MicroServiceClientId = "MicroServiceClientId";
        public static readonly string MicroServiceClientSecret = "MicroServiceClientSecret";

        //Basket PQId's
        public static string RetrieveBasketDetailsPqId = "RetrieveBasketDetailsPqId";
        public static string UpdateContactDetailsPqId = "UpdateContactDetailsPqId";
        public static string UpdatePassengerMuationReqPqId = "UpdatePassengerMuationReqPqId";
        public static string UpdateTnCMutationPqId = "UpdateTnCMutationPqId";

        //RefData PQIds
        public static string RetrieveCountriesPqId = "RetrieveCountriesPqId";
        public static string RetrieveFQTProgrammesPqId = "RetrieveFQTProgrammesPqId";

        public static string RedisConnectionString = "RedisCacheConnectionString";
    }
}
