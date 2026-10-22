namespace VAA.EAPI.Common.Constants
{
    public class Application
    {
        public static readonly string DeltaNoonChar = "N";
        public static readonly string DeltaPMChar = "P";
        public static readonly string ResponseHeaderKey = "ResponseHeaders";
        public static readonly string Subscription = "subscription";
        public static readonly string TechError = "911";
        public static readonly string TechErrorMsg = "Internal error (unknown internal technical error)";
        public static readonly string NotProcessed = "NotProcessed";
        public static readonly string TagText = "Internal technical error";

        public static readonly int Unauthorized = 401;
        public static readonly int Error = 500;
        public static readonly int BusinessException = 422;
    }
}
