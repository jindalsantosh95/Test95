namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Constants
{
    public class Application
    {
        public static readonly string EventSpecversion = "1.0";
        public static readonly string EventUtcTimeFormat = "yyyy-MM-dd HH:mm:ss tt";
        public static readonly string VoidTransactionEventSubject = "VoidTransaction";
        public static readonly string OrderIssueDateFormat = "yyyyMMdd";
        public static readonly string LastTransactionDateFormat = "yyyyMMddHHmmss";
        public static readonly string TicketVoidEventSubject = "VoidTicket";
        public static readonly string TicketFeedFileDelimiter = "\u0001";
        public static readonly string TicketFeedTablesStartWith = "ET_";
        public static readonly string VAASystemProviderCode = "VS";
        public static readonly int AirlineCodeLength = 3;
        public static readonly string CreditCardFraudCheckMessage = "CC CKIN REQUIRED";
        public static readonly string FraudCheckRemarks = "PSGR MUST PRESENT "; 
        public static readonly string TicketFeedDocType = "TicketDocument";
        public static readonly string TicketRetrivalSelectQueryForSkyTeam = "SELECT * FROM c WHERE c.PartitionKey= @TicketNumber or ARRAY_CONTAINS(c.SecondaryTickets, {'EtTktDocSec':  {'TktDocNb':@TicketNumber}}, true)";
        public static readonly string PrimaryTicketDocRetrivalSelectQuery = "SELECT * FROM c WHERE c.PartitionKey= @TicketNumber";
        public static readonly string AssocEmdsRetrivalSelectQueryForSkyTeam = "SELECT doc FROM doc join m IN doc.MiscellaneousDocActivity join md IN m.MiscDocActivityDetail JOIN c IN doc.Coupons JOIN cr IN c.EtCpnRln WHERE md.TDAcTypDtlCd=\"001\" and  cr.FltTktDocNb = @TicketNumber and cr.AssdCpnSttCd =\"A\" and cr.CpnRlnTypCd like '%U%'";
        public static readonly string CheckDocExistPrimaryTicketRetrivalSelectQuery = "SELECT c.DocType, c.id as Id, c.Document.TkDcUpdtSqlNb, c._etag as ETag, c._ts as Ts FROM c WHERE c.PartitionKey= @TicketNumber";
        public static readonly string TicketPurgeEventCapp = "CAPP";

        //Ticket Change Feed
        public static readonly string SchemaVersion = "0.1";
        public const string TicketCollection = "TicketCollection";
        public const string LeaseCollectionName = "LeaseTicketCollection";
        public const string CosmosDBConnectionString = "TicketCosmosDBConnectionString";
        public const int MaxItemsPerInvocation = 10;
        public static readonly string TicketFFPNumberSearchSQL = "SELECT * FROM c WHERE c.PartitionKey = @PartitionKey and c.DocType = @DocType and c.FFPNumber = @FFPNumber";
        public static readonly string TicketEmailSearchSQL = "SELECT * FROM c WHERE c.PartitionKey = @PartitionKey and c.DocType = @DocType and c.ContactEmail = @ContactEmail";
        public static readonly string TicketNumberSearchSQL = "SELECT * FROM c WHERE c.PartitionKey = @PartitionKey and c.DocType = @DocType and c.TicketNumber = @TicketNumber";
        public static readonly string DocTypeFieldName = "DocType";
        public static readonly string PartitionKeyFieldName = "PartitionKey";
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";        
        public const string TicketDocDateFormat = "yyyyMMdd";
    }
}
