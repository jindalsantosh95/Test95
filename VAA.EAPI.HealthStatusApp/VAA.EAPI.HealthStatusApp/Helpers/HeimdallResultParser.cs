using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VAA.EAPI.HealthStatusApp.Helpers
{

    public class HeimdallResultParser
    {
        private static readonly string Pattern = @"\((\d+) passed, (\d+) failed, (\d+) skipped\)";
        private static readonly Regex Regex = new Regex(Pattern);

        public static int GetTotalSuccessfulTests(IEnumerable<string> testResults)
        {
            int totalSuccessful = 0;

            foreach (var result in testResults)
            {
                var match = Regex.Match(result);
                if (match.Success)
                {
                    totalSuccessful += int.Parse(match.Groups[1].Value); 
                }
            }

            return totalSuccessful;
        }

        public static int GetTotalFailedOrSkippedTests(IEnumerable<string> testResults)
        {
            int totalFailedOrSkipped = 0;

            foreach (var result in testResults)
            {
                var match = Regex.Match(result);
                if (match.Success)
                {
                    int failed = int.Parse(match.Groups[2].Value);
                    int skipped = int.Parse(match.Groups[3].Value);
                    totalFailedOrSkipped += failed + skipped;
                }
            }

            return totalFailedOrSkipped;
        }


        public static string ExtractJsonFromString(string input)
        {
            string pattern = @"Response:\s*(\{.*\})";
            Match match = Regex.Match(input, pattern, RegexOptions.Singleline);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty; 
        }


        public static string ExtractDistinctLogString(string input)
        {
            try
            {
                var result = input;
                string[] parts = input.Split(new string[] { "Headers:" }, 2, StringSplitOptions.None);

                if (parts.Length > 1)
                {
                    result = parts[0].Trim() + parts[1].Split(new string[] { "StatusCode" }, 2, StringSplitOptions.None)[1].Trim();
                }

                return RemoveGuidsAndTimeStamp(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while filtering the response: {ex.Message}");
                return input;
            }
        }

        public static string RemoveGuidsAndTimeStamp(string input)
        {
            string guidPattern = @"[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}(-FL-\d+)?";
            string timestampPattern = @"TimeStamp: \d{1,2}/\d{1,2}/\d{4} \d{1,2}:\d{2}:\d{2} (AM|PM)";
            string urlPattern = @"https?://[^\s]+";

            string combinedPattern = $"{guidPattern}|{timestampPattern}|{urlPattern}";

            string result = Regex.Replace(input, combinedPattern, "", RegexOptions.IgnoreCase | RegexOptions.Compiled).Trim();

            result = Regex.Replace(result, @"\s{2,}", " ");
            result = Regex.Replace(result, @"\|\s*\|", "|");
            result = result.Replace("\n", " ");

            return result;
        }
    }
}

