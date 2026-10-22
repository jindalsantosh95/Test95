using System;
using System.Globalization;
using System.Security.Cryptography;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment
{
    public static class Utility
    {
        public static string GetMatchStringDOB(string originalDate, string resultDateFormat)
        {
            return DateTime.Parse(originalDate).ToString(resultDateFormat);
        }
    }
}
