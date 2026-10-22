namespace VAA.EAPI.Microservices.CRM.Constants
{
    public class ErrorCodes
    {
        public static readonly string InvalidBankCountryName = "1081";
        public static readonly string MissingAccountHolderName = "1082";
        public static readonly string MissingInvalidBankName = "1083";
        public static readonly string InvalidCustomerCountryName = "1084";
        public static readonly string DuplicateBankDetails = "10107";

        public static readonly string InvalidPaymentNumber = "10104";
        public static readonly string PaymentUpdateError = "G036";
        public static readonly string MiscTechError = "G037";

        public static readonly string PaymentFetchError = "10105";
        public static readonly string CountryFetchError = "10106";
    }
}
