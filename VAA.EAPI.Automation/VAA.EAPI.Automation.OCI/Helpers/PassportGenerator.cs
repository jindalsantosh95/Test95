using System.Text;

namespace VAA.EAPI.Automation.OCI.Helpers
{
    public static class PassportGenerator
    {
        private static readonly Random _random = new Random();

        // Dictionary defining the format of passport numbers per country
        private static readonly Dictionary<string, Func<string>> PassportFormats =
            new Dictionary<string, Func<string>>(StringComparer.OrdinalIgnoreCase)
        {
        { "USA", () => GenerateUsaPassportNumber() },
        { "GBR", () => RandomDigits(9) },
        { "IND", () => $"{RandomLetter()}{RandomDigits(7)}" },
        { "CAN", () => $"P{RandomDigits(6)}{RandomLetters(2)}" },
        { "AFG", () => RandomDigits(8) },
        { "BRB", () => $"Z{RandomDigits(6)}" },
        { "GRD", () => $"G{RandomDigits(7)}" },
        { "IRN", () => $"XN{RandomDigits(7)}" },
        { "JAM", () => $"A{RandomDigits(7)}" },
        { "D",   () => $"LZ{RandomDigits(4)}{RandomLetter()}{RandomDigits(2)}" },
        { "DEU", () => $"LZ{RandomDigits(4)}{RandomLetter()}{RandomDigits(2)}" },
        { "ATG", () => $"A{RandomDigits(7)}" },
        { "VCT", () => $"RZ{RandomDigits(6)}" },
        { "MDV", () => $"00CC{RandomDigits(5)}" },
        { "KOR", () => $"M{RandomDigits(8)}" },
        { "FRA", () => $"{RandomDigits(2)}{RandomLetters(2)}{RandomDigits(5)}" },
        { "NLD", () => $"SPECI{RandomDigits(4)}" },
        { "ZAF", () => $"ZA{RandomDigits(7)}" },
        { "SAU", () => RandomDigits(9) },
        { "ARE", () => $"A{RandomDigits(8)}" },
        { "MEX", () => $"M{RandomDigits(8)}" },
        { "IRL", () => RandomDigits(8) },
        { "THA", () => $"TH{RandomDigits(7)}" }
        };

        public static string GeneratePassport(string countryCode)
        {
            if (PassportFormats.TryGetValue(countryCode, out var generator))
            {
                return generator();
            }

            throw new ArgumentException($"Unsupported country code: {countryCode}");
        }

        // Helper functions
        private static string RandomDigits(int count)
        {
            string digits = "";
            for (int i = 0; i < count; i++)
                digits += _random.Next(0, 10).ToString();
            return digits;
        }

        private static string RandomLetters(int count)
        {
            string letters = "";
            for (int i = 0; i < count; i++)
                letters += (char)_random.Next('A', 'Z' + 1);
            return letters;
        }

        private static string RandomLetter() => RandomLetters(1);

        private static string GenerateUsaPassportNumber()
        {
            const int totalLength = 9;

            char[] firstChars = { '4', '5', '6', '7', '8', '9', 'A' };

            StringBuilder passport = new StringBuilder(totalLength);

            // First character
            passport.Append(firstChars[_random.Next(firstChars.Length)]);

            // Fill remaining characters to reach length 9
            while (passport.Length < totalLength)
            {
                passport.Append(_random.Next(0, 10));
            }

            return passport.ToString();
        }
    }
}
