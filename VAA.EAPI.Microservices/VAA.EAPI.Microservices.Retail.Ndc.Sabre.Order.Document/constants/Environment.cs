namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Constants
{
    public class Environment
    {
        public static readonly string OrderDataServiceBusConnectionString = "OrderDataServiceBusConnectionString";
        public static readonly string OrderMgtServiceBusConnectionString = "OrderMgtServiceBusConnectionString";

        #region Sabre Variables
        public static readonly string SabreEndpoint = "SabreEndpoint";
        public static readonly string ConversationId = "ConversationId";
        public static readonly string FromPartyId = "FromPartyId";
        public static readonly string ToPartyId = "ToPartyId";
        public static readonly string DirectTicketingPCC = "DirectTicketingPCC";
        #endregion
    }
}
