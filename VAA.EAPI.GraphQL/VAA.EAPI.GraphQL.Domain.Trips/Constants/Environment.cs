using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.Trips.Constants
{
    public class Environment
    {
        public const string KoreanAirlineOperatedHelpText = "TripSettings:KoreanAirlineOperatedHelpText";
        public const string OtherAirlineOperatedHelpText = "TripSettings:OtherAirlineOperatedHelpText";
        public const string HertzManageBookingUrl = "TripSettings:HertzManageBookingUrl";
        public const string FlightOpsMaxConcurrentScheduleRequests = "TripSettings:FlightOpsMaxConcurrentScheduleRequests";
        public const int DefaultFlightOpsMaxConcurrentScheduleRequests = 8;

        public const string PopsOpenDaysBeforeDepartureKey = "TripSettings:PopsOpenDaysBeforeDeparture";
        public const string PopsCloseDaysBeforeDepartureKey = "TripSettings:PopsCloseDaysBeforeDeparture";

        public const int DefaultPopsOpenDaysBeforeDeparture = 7;
        public const int DefaultPopsCloseDaysBeforeDeparture = 1;

        private const string HiddenElementGroupsKey = "TripSettings:HiddenElementGroups";
        private const string DefaultHiddenElementGroups = "FCHIDDEN,OWA,OWAWC,OWAEC,RCD,RETAILBOOK,ATOLLEVY,ADVANCE,LOWDEP,CCBF,STBF,BOOKFEE,HREPFEES";

        public static readonly IReadOnlySet<string> HiddenElementGroups =
            new HashSet<string>((ConfigReader.Try(HiddenElementGroupsKey, DefaultHiddenElementGroups)).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);

        private const string AllowedDocumentTypesKey = "TripSettings:AllowedDocumentTypes";
        private const string DefaultAllowedDocumentTypes = "CBKINV,VATINVOICE,ITINERARYDOCUMENT,BKGCONFEMAIL,ATOLCERTIFICATE,ATOLRECEIPT,TRVDOC,TRVDOCEML,VIAVOUEMAIL";

        public static readonly IReadOnlySet<string> AllowedDocumentTypes =
            new HashSet<string>((ConfigReader.Try(AllowedDocumentTypesKey, DefaultAllowedDocumentTypes)).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);
                                                                                   
    }
}
