using System.Text.RegularExpressions;

namespace VAA.EAPI.Web.Common.Helper
{
    public class RegExHelper
    {
        public static string ConvertToTitle(string inputString)
        {
            return Regex.Replace(inputString, @"\b[a-zA-Z]", m => m.Value.ToUpper());
        }
        public static string RemoveWhiteSpace(string inputString)
        {
            return Regex.Replace(inputString, @"\s", string.Empty);
        }
        public static string RemoveNonAlphaCharacters(string inputString)
        {
            return Regex.Replace(inputString, "[^a-zA-Z]", string.Empty);
        }
        public static string SpecialChacater(string inputString)
        {
            return Regex.Replace(inputString, "[a-zA-Z0-9]+", string.Empty);
        }
        public static string SpecificOneSpecialCharacter(string inputString)
        {
            return Regex.Replace(inputString, "[^@]+", string.Empty);
        }
    }
}
