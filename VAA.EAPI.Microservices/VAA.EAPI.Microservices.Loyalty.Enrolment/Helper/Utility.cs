using System;
using System.Globalization;
using System.Security.Cryptography;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Helper
{
    public static class Utility
    {
        public static string GetGUID()
        {
            Guid guidValue = Guid.NewGuid();
            MD5 md5 = MD5.Create();
            Guid hashed = new Guid(md5.ComputeHash(guidValue.ToByteArray()));
            return hashed.ToString();
        }

        public static string ParseBoolAsString(bool value)
        {
            return value ? "Y" : "N";
        }

        public static string ParseDateZ(string dateString, string outputDateFormat, string acceptedDateFormat)
        {
            return DateTime.TryParseExact(dateString, acceptedDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
                ? result.ToString(outputDateFormat) + "Z"
                : null;
        }

        public static string DateConversion(string originalDate, string OriginalDateFormat, string resultDateFormat)
        {
            string resultDate = string.Empty;
            if (!string.IsNullOrEmpty(originalDate))
            {
                DateTime.TryParseExact(originalDate, OriginalDateFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime outputDate);
                resultDate = outputDate.ToString(resultDateFormat);
            }
            return resultDate;
        }

        public static DateTime? DateFormatConversionToDate(string originalDate, string OriginalDateFormat, string resultDateFormat)
        {
            DateTime? resultDateinDatetimeFormat = null;
            if (!string.IsNullOrEmpty(originalDate))
            {
                DateTime.TryParseExact(originalDate, OriginalDateFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime outputDate);
                string resultDateString = outputDate.ToString(resultDateFormat);

                DateTime.TryParseExact(resultDateString, resultDateFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime outputDate2);
                resultDateinDatetimeFormat = outputDate2;
            }
            return resultDateinDatetimeFormat;
        }

        public static string GetId()
        {
            long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            Random rand = new Random();
            int dRandom = rand.Next(00000000, 99999999);
            string strFinalNum = Convert.ToString(unixTimestamp) + Convert.ToString(dRandom);
            return strFinalNum;
        }

        public static string GetMatchStringDOB(string originalDate, string resultDateFormat)
        {
            return DateTime.Parse(originalDate, new CultureInfo("es-ES")).ToString(resultDateFormat);
        }

        public static DateTime GetLastDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }
    }
}
