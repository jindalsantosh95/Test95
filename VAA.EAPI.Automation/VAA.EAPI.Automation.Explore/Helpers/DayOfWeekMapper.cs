namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class DayOfWeekMapper
    {
        /// <summary>
        /// Maps DB day string ("Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat") to API numeric dayOfWeek (0–6).
        /// </summary>
        public static int ToApiDayOfWeek(string day)
        {
            return day switch
            {
                "Sun" => 0,
                "Mon" => 1,
                "Tue" => 2,
                "Wed" => 3,
                "Thu" => 4,
                "Fri" => 5,
                "Sat" => 6,
                _ => -1
            };
        }
    }
}
