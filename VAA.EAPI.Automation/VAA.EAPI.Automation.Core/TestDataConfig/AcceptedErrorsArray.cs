namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class AcceptedErrorsArray
    {
        //List of accepted errors to be used to exit tests early
        private static readonly Dictionary<string, string[]> acceptedErrorStrings = new Dictionary<string, string[]>
        {
            {"PnrSellError", ["CREATE PNR SELL ERROR -- SPACE UNAVAILABLE"] }
        };

        public static string[] Get(string name)
        {
            if (!acceptedErrorStrings.ContainsKey(name)) throw new KeyNotFoundException($"No entry found with the key name; {name}");

            return acceptedErrorStrings[name];
        }
    }
}
