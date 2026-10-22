namespace VAA.EAPI.GraphQL.Domain.Search.Constants
{
    public static class Application
    {
        public static readonly string BasketIdHeaderContainer = "Headers";
        public static readonly string BasketIdHeader = "BasketId";
        public static readonly string NOT_OFFERED = "NOT_OFFERED";


        #region Sort Fields
        public static readonly Dictionary<string, string> AvailableSortFields = new()
        {
            { "RECOMMENDED", "Recommended" },
            { "PRICE", "Price - lowest to highest" },
            { "DEPARTURE_TIME", "Departure time - earliest to latest" },
            { "DURATION", "Duration - shortest to longest" },
            { "POINTS", "Points - lowest to highest" }
        };
        public static readonly string DEFAULT_SORT = "RECOMMENDED";
        #endregion

        #region Filters
        public static readonly string FILTER_AIRLINE_NAME = "AIRLINE_NAME";
        public static readonly string FILTER_AIRLINE_NAME_LIST_FIRST = "VS";
        public static readonly string FILTER_CONNECTIONS = "CONNECTIONS";
        public static readonly string FILTER_CABIN = "CABIN";
        public static readonly string FILTER_DEPARTURE_TIME = "DEPARTURE_TIME";
        public static readonly string FILTER_SEPARATOR = ".";
        public static readonly string FILTER_PLACEHOLDER = "-";
        public static readonly string FILTER_EARLY_MORNING = "EM";
        public static readonly string FILTER_MORNING = "MO";
        public static readonly string FILTER_AFTERNOON = "AF";
        public static readonly string FILTER_EVENING = "EV";
        public static readonly string FILTER_NIGHT = "NI";

        public static readonly Dictionary<string, string> Filters = new()
        {
            { FILTER_AIRLINE_NAME, "Airline" },
            { FILTER_CONNECTIONS, "Stops" },
            { FILTER_CABIN, "Cabin" },
            { FILTER_DEPARTURE_TIME, "Departure Time" }
        };
        #endregion

        public const string BAGGAGE_TYPE_CHECKED = "Checked";
        public const string BAGGAGE_TYPE_CARRYON = "CarryOn";
        public const int FlightsGoOnSaleFromDays = 331;
        public const string FriendlyDayAndMonth = "d MMM";
        public const string APPLICATION_TYPE = "Free";
        public static readonly string ErrorFilePath = "ErrorCodes\\ErrorCodes.json";
        public static readonly string ISODateOnlyFormat = "yyyy-MM-dd";
        public static readonly string VoucherJourneySectorLimitReasonCode = "VOUCHER_JOURNEY_SECTOR_LIMIT_EXCEEDED";
        public static readonly int MaximumVouchersForRedemption = 2;

        #region ATPCO Award Search 
        public const string AwardSearchPseudoCityCode_ATPCO = "GZC";
        public const string AwardSearchAccountCode_ATPCO = "FCREWARD";
        public const string AwardSearchSource_ATPCO = "1P";
        public const string AwardSearchPTCs_ATPCO = "PFC";
        #endregion ATPCO Award Search 
    }
}
