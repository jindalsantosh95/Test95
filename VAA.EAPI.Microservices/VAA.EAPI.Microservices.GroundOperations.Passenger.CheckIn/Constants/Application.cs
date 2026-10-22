namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants
{
    public static class Application
    {
        public static readonly string DeltaChannelName = "AppChannelName";
        public static readonly string DeltaChannelValue = "DeltaChannelValue";
        public static readonly string DeltaApiId = "AppID";
        public static readonly string DeltaApiIdValue = "DeltaApiIdValue";
        public static readonly string DeltaClientIdName = "ClientID";
        public static readonly string DeltaClientIdValue = "DeltaClientIdValue";
        public static readonly string DeltaTestTrmName = "X-TRRM_VSDigital_Test";
        public static readonly string DeltaTestTrmValue = "DeltaTestTrmValue";
        public static readonly string CheckInStatusCodes = "ErrorCodes/CheckInStatus.json";
        public static readonly string TrmPatchExceptions = "ErrorCodes/TrmPatchExceptions.json";
        public static readonly string ErrorCodesPath = "ErrorCodes/ErrorCodes.json";
        public static readonly string DeltaTrmClientSecret = "DeltaTrmClientSecret";
        public static readonly string ContactTracingInTransit = "Intransit";
        public static readonly string ContactTracingCountry = "USA";
        public static readonly string TrmConfigOptions = "configOptions";
        public static readonly string TrmConfigOptionsValue = "TrmConfigOptionsValue";
        public static readonly string RedirectJVPartner = "redirectJVPartner";
        public static readonly HashSet<string> TravelRequirementExceptionErrors = ["travelrequirement_030_011"];
        public static readonly HashSet<string> PassportValidationErrors = ["travelrequirement_030_001", "travelrequirement_030_002", "travelrequirement_030_003", "travelrequirement_030_005", "travelrequirement_030_006", "travelrequirement_030_008", "travelrequirement_030_025", "travelrequirement_030_026", "travelrequirement_030_029", "travelrequirement_030_030", "travelrequirement_050_002", "travelrequirement_050_003", "travelrequirement_050_004"];
    }
}
