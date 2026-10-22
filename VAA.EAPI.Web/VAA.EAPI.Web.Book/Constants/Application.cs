namespace VAA.EAPI.Web.Book.Constants
{
    public class Application
    {
        public static string KeyVaultUri = "KeyVaultUri";
        public static string KeyVaultTenantID = "KeyVaultTenantId";
        public static string KeyVaultClientID = "KeyVaultClientId";
        public static string KeyVaultClientSecret = "KeyVaultClientSecret";
        public static readonly string Booking = "BOOKING";
        public const string PaymentStatus = "Pending";
        public const string Type = "CPD";
        public const string ResourceType = "PAYMENT_SESSION";
        public static readonly string DefalutDeviceInfo = "PDWDC";
        public const string FraudCheckErrorCode = "2001";
        public const string VoidErrorCode = "2002";
        public const string CaptureErrorCode = "2002";
        public static readonly string ErrorFilePath = "ErrorCodes\\ErrorCodes.json";
    }
}
