namespace VAA.EAPI.Microservices.Loyalty.Wallet.Constants
{
    public static class Application
    {
        public static readonly string passDescription = "Flying Club Membership Card";
        public static readonly string passOrganization = "VirginAtlantic";
        public static readonly string TierPoints = "TIER POINTS";
        public static readonly string Name = "NAME";
        public static readonly string MemberShipNumber = "MEMBERSHIP NUMBER";
        public static readonly string ValidUntil = "VALID UNTIL";
        public static readonly string LableColor = "rgb(255, 255, 255)";
        public static readonly string ForegroundColor = "rgb(255, 255, 255)";
        public static readonly string WalletPassEvent = "WalletPass";
        public static readonly string SerialNumber = "SerialNumber";
        public static string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static readonly string GetCustomerDetails = "customer/{customerId}";
        public static readonly string GetCustomerDtlsByMemberID = "customer/loyalty/{memberid}";
        public static readonly string GetFcMemberDetails = "flyingClubMember/{customerId}";
        public static readonly string ContenTypeapplicationjson = "application/json";
        public const string AppleWalletTierEventHandlerTopicName = "auditeventmessages";
        public const string CustomerTierSubscriber = "CreateCustomerTierAuditEventSubscriber";
        public static readonly string Title = "title";
        public static readonly string Body ="body";
        public static readonly string Separator = "-";
        public static readonly string DeviceId = "DeviceId";
        public const string MemberTierUpdateEventTopicName = "customertransactioneventmessages";
        public const string WalletTierUpdateSubscriber = "WalletTierUpdateSubscriber";
        public const string WalletNotification = "WalletNotification";
        public const string WalletNotificationUpdateSubscriber = "WalletNotificationUpdateSubscriber";
        public const string IconImage = "icon.png";
        public const string Icon2XImage = "icon@2x.png";
        public const string Icon3XImage = "icon@3x.png";
        public const string RedLogoImage = "VaaRedlogo.png";
        public const string RedLogo2XImage = "VaaRed2Xlogo.png";
        public const string RedLogo3XImage = "VaaRed3Xlogo.png";
        public const string GoldLogoImage = "VaaGoldlogo.png";
        public const string GoldLogo2XImage = "VaaGold2Xlogo.png";
        public const string GoldLogo3XImage = "VaaGold3Xlogo.png";
        public const string SilverLogoImage = "VaaSilverlogo.png";
        public const string SilverLogo2XImage = "VaaSilver2Xlogo.png";
        public const string SilverLogo3XImage = "VaaSilver3Xlogo.png";
        public const string RedTierImage = "strip-red.png";
        public const string RedTier2XImage = "strip-red@2x.png";
        public const string RedTier3XImage = "strip-red@3x.png";
        public const string GoldTierImage = "strip-gold.png";
        public const string GoldTier2XImage = "strip-gold@2x.png";
        public const string GoldTier3XImage = "strip-gold@3x.png";
        public const string SilverTierImage = "strip-silver.png";
        public const string SilverTier2XImage = "strip-silver@2x.png";
        public const string SilverTier3XImage = "strip-silver@3x.png";
        public static readonly string ValidUntilDateFormat = "dd/MM/yyyy";
        public static readonly string DataVersion = "0.1";
        public static readonly string AirshipTokenHeaderName = "Authorization";
        public static readonly string IsoLocalDateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss";
        public const string IosDeviceType = "ios";
        public const string AndroidDeviceType = "android";
    }
}
