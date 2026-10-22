namespace VAA.EAPI.Microservices.FlightOperations.Flights.Constants
{
    public static class Environment
    {
        public static readonly string OperationalScheduleOcpKey = "FlightOperation:APIM:OcpSubscriptionKey";
        public static readonly string OperationalScheduleUrl = "FlightOperation:APIM:Url";
        public static readonly string OperationalScheduleAPIClientId = "FlightOperation:OperationalScheduleAPIClientId";
        public static readonly string OperationalScheduleAPIClientSecret = "FlightOperation:OperationalScheduleAPIClientSecret";
        public static readonly string OperationalScheduleAPIAuthority = "FlightOperation:OperationalScheduleAPIAuthority";
        public static readonly string OperationalScheduleAPIResource = "FlightOperation:OperationalScheduleAPIResource";
        public static readonly string AircraftCategorySqlDbConnectionString = "FlightOperation:AircraftCategorySqlDbConnectionString";
        public static readonly string FlightStatusDeltaServiceLayerBaseUrl = "FlightStatus:DeltaServiceLayerBaseUrl";
    }
}
