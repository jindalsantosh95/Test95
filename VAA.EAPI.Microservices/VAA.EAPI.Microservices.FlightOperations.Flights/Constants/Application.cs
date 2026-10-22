namespace VAA.EAPI.Microservices.FlightOperations.Flights.Constants
{
    public static class Application
    {
        public static readonly string[] DateOnlyIncomingFormats = ["dd/MM/yyyy", "dd/M/yyyy", "d/MM/yyyy", "d/M/yyyy"];
        public static readonly string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static readonly string CheckAircraftCategoryQuery = "SELECT DISTINCT RTRIM(ACREG) AS TailNumber FROM [dbo].[ACCATEG] WHERE [CATEGIDX] = @categoryIndex AND [ACREG] IN @tailNumbers";
    }
}
