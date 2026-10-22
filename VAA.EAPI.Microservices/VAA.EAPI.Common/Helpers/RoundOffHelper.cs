using System;

namespace VAA.EAPI.Common.Helpers
{
    public static class RoundOffHelper
    {
        public static decimal RoundTo2(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
