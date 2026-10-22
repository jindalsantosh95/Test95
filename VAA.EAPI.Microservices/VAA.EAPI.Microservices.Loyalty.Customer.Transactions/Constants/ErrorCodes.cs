namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants
{
    public class ErrorCodes
    {
        // General Error Codes
        public static readonly string CustomerIdNotFoundMessage = "CustomerID not found";
        public static readonly string ReadRequestError = "G007";
        public static readonly string DBConnectionError = "G008";
        public static readonly string MiscellaneousTechnicalError = "G009";
        public static readonly string RequestValidationError = "G021";
        public static readonly string CommandResponseStatusNotReady = "G027";
        public static readonly string GroupIdNotFoundMessage = "GroupID not found";
        public static readonly string GroupDocumentNotFoundMessage = "Group Document not found";

        // Contact Note Error Codes
        public static readonly string ContactNoteTypeGroupInvalid = "1034";
        public static readonly string ContactNoteTypeInvalid = "1035";
        public static readonly string ContactNoteTextInvalid = "1036";
        public static readonly string ContactNoteIdInvalid = "1037";


        // Targeted Membership Error Codes
        public static readonly string TargetedMembershipIdInvalid = "3333";

        // Transaction Doc Error Codes
        public static readonly string DuplicateTransaction = "2001";
        public static readonly string InCorrectPreferenceValue = "10161c";
        public static readonly string DuplicateTransactionVocuherAccrual = "2002";

        //PartnerMembership
        public static readonly string MandatoryInputFieldsMissing = "15001";
        public static readonly string DuplicateCardFound = "G031";
        public static readonly string PartnerMembershipIdInvalid = "3334";
        public static readonly string InvalidPreferredPartnerFFP = "10200";

        //Cancel Transaction
        public static readonly string TransactionIdNotFoundMessage = "800";
        public static readonly string InvalidCancellationTransaction = "815";

        //Award
        public static readonly string AwardIdInvalid = "3335";

        public static readonly string VoucherTxnPnrUpdateCommandStatusError = "2002";
    }
}