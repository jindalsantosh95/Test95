using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderCreate;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public static class GenerateDataHelper
    {
        private static Random rnd = new Random();

        public static string Gender()
        {
            string[] genderTypes = { "M", "F", "U", "X" };
            int index = rnd.Next(0, genderTypes.Length);
            return genderTypes[index];
        }

        public static string DateOfBirth(string paxType)
        {
            switch (paxType)
            {
                case "ADT":
                    return DateTime.Now.AddYears(-rnd.Next(16, 100)).ToString("yyyy-MM-dd");

                case "CHD":
                    return DateTime.Now.AddYears(-rnd.Next(2, 11)).ToString("yyyy-MM-dd");

                case "GBE":
                    return DateTime.Now.AddYears(-rnd.Next(12, 15)).ToString("yyyy-MM-dd");

                case "INF":
                    return DateTime.Now.AddDays(-rnd.Next(1, 365)).ToString("yyyy-MM-dd");

                default: throw new ArgumentException(nameof(paxType));
            }
        }

        public static string RandomDate(string startdate, int maxNumberOfDays)
        {
            DateTime date = DateTime.ParseExact(startdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            Random random = new Random();

            DateTime newDate = date.AddDays(random.Next(1, maxNumberOfDays));

            string journey1Date = newDate.ToString("yyyy-MM-dd");

            return journey1Date;
        }
    }
}
