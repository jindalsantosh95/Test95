using System;

namespace VAA.EAPI.Common.Helpers
{
    public static class Utility
    {
        public static string GetId()
        {
            long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            Random rand = new Random();
            int dRandom = rand.Next(00000000, 99999999);
            string strFinalNum = Convert.ToString(unixTimestamp) + Convert.ToString(dRandom);
            return strFinalNum;
        }
        public static string GenerateMatchString(string customerType, string streetName, string postCode, string firstName, string lastName, string countryCode, string DOB)
        {
            string matchString = string.Empty;
            streetName = streetName?.Replace(" ", string.Empty);
            postCode = postCode?.Replace(" ", string.Empty);
            switch (customerType)
            {
                case "Basic":
                case "VirginAccount":
                    if (string.IsNullOrEmpty(streetName) && string.IsNullOrEmpty(postCode) && string.IsNullOrEmpty(countryCode))
                    {
                        matchString = firstName.Substring(0, 1) + lastName + DOB;
                    }
                    break;
                default:
                    if (!string.IsNullOrEmpty(postCode))
                    {
                        if (streetName.Length >= 6)
                        {
                            matchString = postCode.Length >= 4
                            ? (firstName.Substring(0, 1) + lastName + streetName.Substring(0, 6) + postCode.Substring(0, 4) + countryCode.Substring(0, 2) + DOB)
                            : (firstName.Substring(0, 1) + lastName + streetName.Substring(0, 6) + countryCode.Substring(0, 2) + DOB);
                        }
                        else
                        {
                            matchString = postCode.Length >= 4
                            ? (firstName.Substring(0, 1) + lastName + postCode.Substring(0, 4) + countryCode.Substring(0, 2) + DOB)
                            : (firstName.Substring(0, 1) + lastName + countryCode.Substring(0, 2) + DOB);
                        }
                    }
                    else
                    {
                        if (streetName.Length >= 6)
                        {
                            matchString = firstName.Substring(0, 1) + lastName + streetName.Substring(0, 6) + countryCode.Substring(0, 2) + DOB;
                        }

                    }
                    break;
            }
            return matchString;
        }

    }
}
