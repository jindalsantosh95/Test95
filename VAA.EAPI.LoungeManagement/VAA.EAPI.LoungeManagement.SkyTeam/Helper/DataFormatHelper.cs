using System;
using System.Globalization;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Helper
{
    public static class DataFormatHelper
    {
        public static string ConvertToTimeFormat(string time)
        {
            if (string.IsNullOrEmpty(time))
            {
                return string.Empty;
            }
            else
            {
                string period = time.Substring(time.Length - 1).ToUpper();
                time = time.Substring(0, time.Length - 1);
                time = time.PadLeft(4, '0');
                time = time.Insert(time.Length - 2, ":");
                DateTime parsedTime = DateTime.ParseExact(time + " " + period + "M", "hh:mm tt", CultureInfo.InvariantCulture);

                return parsedTime.ToString("HH:mm:ss");
            }

        }
    }
}
