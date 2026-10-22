
namespace VAA.EAPI.Automation.Core.UI.Xunit.Features
{
    public static class StepText
    {
        private static readonly string[] Prefixes = { "Given ", "When ", "Then ", "And ", "But " };

        public static string Normalize(string stepLine)
        {
            var s = stepLine.Trim();

            foreach (var p in Prefixes)
                if (s.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                    return s.Substring(p.Length).Trim();

            return s;
        }
    }
}