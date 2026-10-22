namespace VAA.EAPI.GraphQL.Domain.Account.Helpers
{
    public static class Utility
    {
        public static string GetVredTransactionId()
        {
            long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            Random rand = new Random();
            int dRandom = rand.Next(00000000, 99999999);
            string strFinalNum = Convert.ToString(unixTimestamp) + Convert.ToString(dRandom);
            return strFinalNum;
        }
    }
}
