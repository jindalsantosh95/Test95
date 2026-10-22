using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;

namespace VAA.EAPI.Automation.Core.Text
{
    public static class RegexCompiled
    {
        private static readonly ConcurrentDictionary<string, Regex> _cache = [];

        public static Match Match(string input, string pattern) => Match(input, pattern, RegexOptions.Compiled);
        public static Match Match(string input, string pattern, RegexOptions options) => Regex.Match(input, pattern, RegexOptions.Compiled | options);

        public static MatchCollection Matches(string input, string pattern) => Matches(input, pattern, RegexOptions.Compiled);
        public static MatchCollection Matches(string input, string pattern, RegexOptions options) => Regex.Matches(input, pattern, RegexOptions.Compiled | options);

        public static bool IsMatch(string input, string pattern) => IsMatch(input, pattern, RegexOptions.Compiled);
        public static bool IsMatch(string input, string pattern, RegexOptions options) => Regex.IsMatch(input, pattern, RegexOptions.Compiled | options);

        public static string Replace(string input, string pattern, string replacement) => Replace(input, pattern, replacement, RegexOptions.Compiled);
        public static string Replace(string input, string pattern, string replacement, int count) => Replace(input, pattern, replacement, RegexOptions.Compiled, count);
        public static string Replace(string input, string pattern, string replacement, RegexOptions options) => Replace(input, pattern, replacement, RegexOptions.Compiled | options, 0);
        public static string Replace(string input, string pattern, string replacement, RegexOptions options, int count)
        {
            if (count > 0)
            {
                Regex regex = _cache.GetOrAdd($"{pattern}_{options}", _ => new Regex(pattern, RegexOptions.Compiled | options));

                return regex.Replace(input, replacement, count);
            }

            return Regex.Replace(input, pattern, replacement, RegexOptions.Compiled | options);
        }
        public static string Replace(string input, string pattern, MatchEvaluator evaluator) => Replace(input, pattern, evaluator, RegexOptions.Compiled);
        public static string Replace(string input, string pattern, MatchEvaluator evaluator, RegexOptions options) => Replace(input, pattern, evaluator, RegexOptions.Compiled | options, 0);
        public static string Replace(string input, string pattern, MatchEvaluator evaluator, RegexOptions options, int count)
        {
            if (count > 0)
            {
                Regex regex = _cache.GetOrAdd($"{pattern}_{options}", _ => new Regex(pattern, RegexOptions.Compiled | options));

                return regex.Replace(input, evaluator, count);
            }

            return Regex.Replace(input, pattern, evaluator, RegexOptions.Compiled | options);
        }

        public static string[] Split(string input, string pattern) => Split(input, pattern, RegexOptions.Compiled);
        public static string[] Split(string input, string pattern, RegexOptions options) => Split(input, pattern, RegexOptions.Compiled | options, 0);
        public static string[] Split(string input, string pattern, RegexOptions options, int count)
        {
            if (count > 0)
            {
                Regex regex = _cache.GetOrAdd($"{pattern}_{options}", _ => new Regex(pattern, RegexOptions.Compiled | options));

                return regex.Split(input, count);
            }

            return Regex.Split(input, pattern, RegexOptions.Compiled | options);
        }

        public static class Try
        {
            public static bool Match(string input, string pattern, out Match match) => Match(input, pattern, RegexOptions.Compiled, out match);
            public static bool Match(string input, string pattern, RegexOptions options, out Match match)
            {
                match = default;

                try
                {
                    match = RegexCompiled.Match(input, pattern, options);

                    return match.Success;
                }
                catch
                {
                    return false;
                }
            }

            public static bool Matches(string input, string pattern, out MatchCollection matches) => Matches(input, pattern, RegexOptions.Compiled, out matches);
            public static bool Matches(string input, string pattern, RegexOptions options, out MatchCollection matches)
            {
                matches = default;

                try
                {
                    matches = RegexCompiled.Matches(input, pattern, options);

                    return matches.Any(m => m.Success);
                }
                catch
                {
                    return false;
                }
            }

            public static bool IsMatch(string input, string pattern, out bool? isMatch) => IsMatch(input, pattern, RegexOptions.Compiled, out isMatch);
            public static bool IsMatch(string input, string pattern, RegexOptions options, out bool? isMatch)
            {
                isMatch = default;

                try
                {
                    isMatch = RegexCompiled.IsMatch(input, pattern, options);

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static bool Replace(string input, string pattern, string replacement, out string value) => Replace(input, pattern, replacement, RegexOptions.Compiled, out value);
            public static bool Replace(string input, string pattern, string replacement, int count, out string value) => Replace(input, pattern, replacement, RegexOptions.Compiled, count, out value);
            public static bool Replace(string input, string pattern, string replacement, RegexOptions options, out string value) => Replace(input, pattern, replacement, RegexOptions.Compiled | options, 0, out value);
            public static bool Replace(string input, string pattern, string replacement, RegexOptions options, int count, out string value)
            {
                value = default;

                try
                {
                    value = RegexCompiled.Replace(input, pattern, replacement, options, count);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            public static bool Replace(string input, string pattern, MatchEvaluator evaluator, out string value) => Replace(input, pattern, evaluator, RegexOptions.Compiled, out value);
            public static bool Replace(string input, string pattern, MatchEvaluator evaluator, RegexOptions options, out string value) => Replace(input, pattern, evaluator, RegexOptions.Compiled | options, 0, out value);
            public static bool Replace(string input, string pattern, MatchEvaluator evaluator, RegexOptions options, int count, out string value)
            {
                value = default;

                try
                {
                    value = RegexCompiled.Replace(input, pattern, evaluator, options, count);

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static bool Split(string input, string pattern, out string[] values) => Split(input, pattern, RegexOptions.Compiled, out values);
            public static bool Split(string input, string pattern, RegexOptions options, out string[] values) => Split(input, pattern, RegexOptions.Compiled | options, 0, out values);
            public static bool Split(string input, string pattern, RegexOptions options, int count, out string[] values)
            {
                values = default;

                try
                {
                    values = RegexCompiled.Split(input, pattern, options, count);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
