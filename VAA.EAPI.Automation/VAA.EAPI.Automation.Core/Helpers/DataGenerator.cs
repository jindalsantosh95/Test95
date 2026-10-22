using System.Data;
using System.Globalization;
using RandomNameGeneratorLibrary;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public sealed class DataGenerator
    {
        private static readonly PersonNameGenerator personGenerator = new();

        private static readonly object PersonGeneratorLock = new();

        public static string RandomFirstName()
        {
            lock (PersonGeneratorLock)
                return personGenerator.GenerateRandomFirstName();
        }

        public static string RandomLastName()
        {
            lock (PersonGeneratorLock)
                return personGenerator.GenerateRandomLastName();
        }

        /// <summary>
        /// Returns a random string with Specified length. Pass true for alphaNumeric param if you need alphanumeric string
        /// </summary>

        public static string RandomString(int length, bool alphaNumeric = false)
        {
            string chars = "ABCDEFGHUJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            if (alphaNumeric)
            {
                chars += "0123456789";
            }
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Returns a random number in string format. You will need to convert to int if required as int
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="padLeft"></param>
        /// <returns></returns>
        public static string RandomNumber(long start = 0, long end = 1000000000, bool trimLeadingZeros = false)
        {
            if (trimLeadingZeros)
                return new string(Random.Shared.NextInt64(start, end).ToString("D10").TrimStart('0'));
            else
                return new string(Random.Shared.NextInt64(start, end).ToString("D10"));
        }

        public static DataTable LoadDataSource(string path, bool hasFieldsEnclosedInQuotes = false, bool hasHeaders = true)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            FileInfo fileInfo = new FileInfo(path);

            path = fileInfo.FullName;

            if (!fileInfo.Exists)
                throw new InvalidOperationException($"Data source path does not exist {path}");

            DataTable dataTable = new DataTable();

            dataTable.FromDelimitedFile(path, ",", hasFieldsEnclosedInQuotes, hasHeaders);

            return dataTable;
        }

        /// <summary>
        /// Returns Random Date from the Past if days=0 else returns the unique date w.r.t to days value passed .Only +ve allowed
        /// </summary>
        public static DateTime RandomPastDate(int days = 0)
        {
            DateTime startDate = (days != 0) ? DateTime.UtcNow.AddDays(-days) : new DateTime(1995, 1, 1);
            int range = (DateTime.Today - startDate).Days;

            return startDate.AddDays(Random.Shared.Next(range));
        }

        /// <summary>
        /// Returns Random Date from the Future if days=0 else returns the unique date w.r.t to days value passed .Only +ve allowed
        /// </summary>
        public static DateTime RandomFutureDate(int days = 0)
        {
            DateTime startDate = new(1995, 1, 1);
            int range = (days != 0) ? days : (DateTime.Today - startDate).Days;

            return DateTime.Today.AddDays(Random.Shared.Next(1, range));
        }

        /// <summary>
        /// Returns Random Date between the given startDate and endDate ; startDate must be olderDate than endDate
        /// </summary>
        public static DateTime RandomDateBetweenTwoDates(DateTime startDate, DateTime endDate)
        {
            int range = (endDate - startDate).Days;

            return startDate.AddDays(Random.Shared.Next(range));
        }

        /// <summary>
        /// Returns a departure date within the next 5 - 10 months and a randomly selected day of that month.
        /// </summary>
        /// <returns></returns>
        public static string DepartureDate()
        {
            Random rnd = new();
            DateTime randomMonth = DateTime.Now.AddMonths(rnd.Next(5, 10));

            // Get the total number of days in the selected month
            int daysInMonth = DateTime.DaysInMonth(randomMonth.Year, randomMonth.Month);

            // Generate a random day within the selected month
            int randomDay = rnd.Next(1, daysInMonth + 1);

            // Create the final random date
            DateTime departureDate = new(randomMonth.Year, randomMonth.Month, randomDay);

            // Format the date as "yyyy-MM-ddTHH:mm:ss"
            string formattedDate = departureDate.ToString("yyyy-MM-ddT00:00:00", System.Globalization.CultureInfo.InvariantCulture);

            return formattedDate;
        }

        /// <summary>
        /// Returns a return date which will be 7 days after the provided departure date.
        /// </summary>
        public static string ReturnDate(string departureDate) => departureDate.Parse<DateTime>().AddDays(7).ToString("yyyy-MM-ddT00:00:00", System.Globalization.CultureInfo.InvariantCulture);

        public static DateTime RandomDateOfBirth(bool isMinor = false)
        {
            if (isMinor)
            {
                int minorYears = Random.Shared.Next(1, 18);
                return DateTime.Today.AddYears(-minorYears).AddDays(-Random.Shared.Next(0, 365));
            }
            else
            {
                int adultYears = Random.Shared.Next(18, 100);
                return DateTime.Today.AddYears(-adultYears).AddDays(-Random.Shared.Next(0, 365));
            }
        }

        public static string GetDepartureDate(string dayOfTheWeek)
        {
            if (string.IsNullOrWhiteSpace(dayOfTheWeek))
                throw new ArgumentException("Day of the week must be provided.", nameof(dayOfTheWeek));

            if (!Enum.TryParse<DayOfWeek>(dayOfTheWeek, true, out System.DayOfWeek targetDay))
                throw new ArgumentException($"Invalid day of the week: {dayOfTheWeek}", nameof(dayOfTheWeek));

            DateTime today = DateTime.UtcNow.Date;
            int daysUntilTarget = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilTarget == 0)
                daysUntilTarget = 7; // always pick next occurrence, not today if today is the target day
            DateTime nextToNextTarget = today.AddDays(daysUntilTarget + 28); // get the target day + 28 days
            return nextToNextTarget.ToString("yyyy-MM-ddT00:00:00", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string DepartureDate(params DayOfWeek[] validDays)
        {
            Random rnd = new();
            DateTime departureDate;

            if (validDays != null && validDays.Length > 0)
            {
                // Generic future window: 3 to 10 months from now
                DateTime startDate = DateTime.Now.AddMonths(3).Date;
                DateTime endDate = DateTime.Now.AddMonths(10).Date;
                int totalDays = (endDate - startDate).Days;

                do
                {
                    departureDate = startDate.AddDays(rnd.Next(totalDays + 1));
                }
                while (!validDays.Contains(departureDate.DayOfWeek));
            }
            else
            {
                // General random future date (5–10 months from now)
                DateTime randomMonth = DateTime.Now.AddMonths(rnd.Next(5, 10));
                int daysInMonth = DateTime.DaysInMonth(randomMonth.Year, randomMonth.Month);
                int randomDay = rnd.Next(1, daysInMonth + 1);

                departureDate = new DateTime(randomMonth.Year, randomMonth.Month, randomDay);
            }

            return departureDate.ToString("yyyy-MM-ddT00:00:00", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a random 10-digit phone number without a country code.
        /// </summary>
        public static string RandomPhoneNumber()
        {
            return Random.Shared.NextInt64(1_000_000_000, 10_000_000_000).ToString(CultureInfo.InvariantCulture);
        }

        public static string RandomPhoneCountryCode()
        {
            string[] countryCodes = { "1", "44", "91", "61", "81", "49", "33", "86", "7", "39" };

            return countryCodes[Random.Shared.Next(countryCodes.Length)];
        }

        public static string GetRandomDateWithinNextMonth()
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddMonths(1);
            int range = (endDate - startDate).Days;

            DateTime randomDate = startDate.AddDays(Random.Shared.Next(range + 1));

            return randomDate.ToString("yyyy-MM-dd");
        }

        public static string GetRandomFuturePassportExpiryDate()
        {
            DateTime today = DateTime.Today;
            DateTime minDate = today.AddYears(5);
            DateTime maxDate = today.AddYears(10);

            int daysRange = (maxDate - minDate).Days;
            DateTime randomDate = minDate.AddDays(Random.Shared.Next(daysRange + 1));

            return randomDate.ToString("yyyy-MM-dd");
        }

        public static string GetRandomGenderCode()
        {
            string[] genderCodes = { "M", "F", "X" };

            return genderCodes[Random.Shared.Next(genderCodes.Length)];
        }

        public static string GenerateRandomEmail(int localPartLength = 6, string domain = "example.com")
        {
            const string allowedChars = "abcdefghijklmnopqrstuvwxyz0123456789";

            string localPart = new string(Enumerable
                .Range(0, localPartLength)
                .Select(_ => allowedChars[Random.Shared.Next(allowedChars.Length)])
                .ToArray());

            return $"{localPart}@{domain}";
        }

        /// <summary>
        /// Returns a random date from the specified year. The date will be between January 1st and December 31st of that year.
        /// </summary>
        public static DateTime GetRandomDateFromYear(int year)
        {
            // Get the first and last day of the year
            DateTime start = new(year, 1, 1);
            DateTime end = new(year, 12, 31);

            if (year == DateTime.Today.Year)
            {
                end = new(year, DateTime.Today.Month, DateTime.Today.Day);
            }

            // Total days in the year
            int range = (end - start).Days + 1;

            // Pick a random offset
            return start.AddDays(Random.Shared.Next(range));
        }

        private static readonly string[] Iso2CountryCodes = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(culture => new RegionInfo(culture.Name).TwoLetterISORegionName)
            .Where(countryCode => countryCode.Length == 2 && countryCode.All(char.IsLetter))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        public static string RandomISO2CountryCode()
        {
            return Iso2CountryCodes[Random.Shared.Next(Iso2CountryCodes.Length)];
        }
    }
}
