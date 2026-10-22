using System;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Helper
{
    public static class DecimalHelper
    {
        public static string GetDecimalFormat(string input)
        {
            decimal decimalInput = decimal.Parse(input);
            int decimalPlaces = BitConverter.GetBytes(decimal.GetBits(decimalInput)[3])[2];

            return string.Format("{{0:F{0}}}", decimalPlaces);
        }
    }
}
