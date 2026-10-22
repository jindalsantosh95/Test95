using System.Text.RegularExpressions;

namespace VAA.EAPI.Common.Helpers
{
    public static class RegExHelper
    {
        public static string ConvertToTitle(string inputString) => RegexCompiled.Replace(inputString, @"\b[a-zA-Z]", m => m.Value.ToUpper());
        public static string RemoveWhiteSpace(string inputString) => RegexCompiled.Replace(inputString, @"\s", string.Empty);
        public static string RemoveNonAlphaCharacters(string inputString) => RegexCompiled.Replace(inputString, "[^a-zA-Z]", string.Empty);
        public static string SpecialCharacter(string inputString) => RegexCompiled.Replace(inputString, "[a-zA-Z0-9]+", string.Empty);
        public static string SpecificOneSpecialCharacter(string inputString) => RegexCompiled.Replace(inputString, "[^@]+", string.Empty);
        public static string RemoveSpaceMorethanOnce(string inputString) => RegexCompiled.Replace(inputString, "[ ]{2,}", @" ");
        public static string RemoveSpecificCharacter(string inputString) => RegexCompiled.Replace(inputString, @"[!@#$%\^&*\(\)+=\/\\\{\}\[\]\|:;\<>,.\?\~`;]", string.Empty);
        public static string RemoveSpecialChar(string inputString) => RegexCompiled.Replace(inputString, @"[^a-zA-Z0-9 ./\\-]", string.Empty);
        public static string RemoveSpecialCharacter(string inputString) => RegexCompiled.Replace(inputString, @"[-!@#$%\^&*\(\)+=\/\\\{\}\[\]\|:;\<>,.\?\~`;]", string.Empty);
    }
}
