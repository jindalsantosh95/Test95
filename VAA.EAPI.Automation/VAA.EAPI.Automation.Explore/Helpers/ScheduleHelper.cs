namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class ScheduleHelper
    {
        private static readonly string[] OpenOptions = { "09:00", "08:00" };

        private static readonly string[] CloseOptions = { "19:00", "21:00" };

        public static (int[] days, string[] opens, string[] closes) GenerateRandomSchedule(int numberOfDays)
        {
            if (numberOfDays < 1 || numberOfDays > 7)
                throw new ArgumentOutOfRangeException(nameof(numberOfDays), numberOfDays, "numberOfDays must be between 1 and 7.");

            int[] days = Enumerable.Range(0, 7).OrderBy(_ => Random.Shared.Next()).Take(numberOfDays).ToArray();

            string[] opens = days.Select(_ => OpenOptions[Random.Shared.Next(OpenOptions.Length)]).ToArray();

            string[] closes = days.Select(_ => CloseOptions[Random.Shared.Next(CloseOptions.Length)]).ToArray();

            return (days, opens, closes);
        }
    }
}
