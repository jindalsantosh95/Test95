using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Helper
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
    }
}
