namespace VAA.EAPI.Automation.Explore.Constants
{
    public static class Application
    {        
        public const string ServiceKids = "Kids Area";

        public const string ServiceFood = "Welcome Drinks and Snacks";

        public const string ServiceAircraft = "Aircraft Seats";

        public const string ServiceCinema = "Secret Cinema";

        // Case-insensitive service comparison
        public static bool ContainsService(IEnumerable<string>? services, string expected)
        {
            return services != null && services.Contains(expected, StringComparer.OrdinalIgnoreCase);
        }

        // Closure Types
        public const string ClosureFullDay = "FULL_DAY";

        // Closure Reasons
        public const string ClosureTestReason = "Closure Test Day in Local";
    }
}
