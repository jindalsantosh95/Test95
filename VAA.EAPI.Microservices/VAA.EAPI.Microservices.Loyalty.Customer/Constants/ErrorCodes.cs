namespace VAA.EAPI.Microservices.Loyalty.Customer.Constants
{
    public class ErrorCodes
    {
        public static readonly string InflightExceptionFilePath = "Infrastructure\\Exceptions\\InflightCustomer\\ErrorConfig.json";
        public static readonly string CustomerProfileBusinessException = "G007";
        public static readonly string InvalidCustomerId = "1001";
        public static readonly string InvalidMemberStatus = "P003";
        public static readonly string NoRecordsErrorCode = "G007";
        public static readonly string TopMemberErrorCode = "1041";
        public static readonly string EmailidMatchError = "207";
        public static readonly string EmailidDuplicateError = "1011";
        public static readonly string MatchStringError = "9916";
        public static readonly string BothEmailidMatchstringError = "209";
        public static readonly string VredError1 = "1038";
        public static readonly string VredError2 = "1039";
        public static readonly string VredError3 = "1040";
        public static readonly string MiscellaneousTechnicalError = "G009";
        public static readonly string Memberstatus = "10139";
        public static readonly string InvalidChannelError = "10002";
        public static readonly string FirstNameMissing = "G014";
        public static readonly string LastNameMissing = "G015";
        public static readonly string AddressLine1Missing = "G016";
        public static readonly string AddressTypeMissing = "G017";
        public static readonly string CountryMissing = "G018";
        public static readonly string CityMissing = "G019";
        public static readonly string DOBMissing = "G020";
        public static readonly string CustomerTypeMissing = "1032";
        public static readonly string PhoneTypeMissing = "1033";
        public static readonly string Unauthorized = "401";
        public static readonly string MandatoryInputFieldsMissing = "15001";
        public static readonly string InvalidPreferredPartnerFFP = "10200";
        public static readonly string InvalidChannel = "15002";
        public static readonly string InvalidTitle = "16";
        public static readonly string InvalidMediaCode = "17";
        public static readonly string InvalidCountryCode = "18";
        public static readonly string MissingChanneluserIDError = "1026";
        public static readonly string InvalidAgeError = "1027";
        public static readonly string InvalidMemberType = "21";
        public static readonly string InvalidCustomerAge = "22";
        public static readonly string InvalidParentAge = "223";
        public static readonly string InvalidDateFromat = "20";
        public static readonly string InvalidEmailFromat = "19";
        public static readonly string FCGenNumberTechnicalException = "901";
        public static readonly string BlacklistedEmailidError = "1016";
        public static readonly string EmailCheckForBasicAccount = "G038";
        public static readonly string EmailExistsInCosmosDB = "10161a";
        public static readonly string DeDupValidationError = "10190";
        public static readonly string CustomerAgeLimitError = "10191";
        public static readonly string InvalidCharacterInputFields = "10193";
        public static readonly string InvalidCharacterLenghth = "10194";
        public static readonly string AgeRestrictionError = "10195";
        public static readonly string VACRestrictionError = "10174";
    }
}
