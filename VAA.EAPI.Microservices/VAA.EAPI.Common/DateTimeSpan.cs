using System;
using System.Diagnostics.CodeAnalysis;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Common
{
    public sealed class DateTimeSpan : IParsable<DateTimeSpan>
    {
        private const string TimeSpanSyntaxRegex = @"(^-|^)(\d{2})?(:\d{2}){2,}";
        private const string FrequencyCountSyntaxRegex = @"(\d+):(day|week|month|year)";

        public enum Frequency
        {
            Unknown = 0,
            Day,
            Week,
            Month,
            Year,
        }

        public static explicit operator DateTimeSpan(TimeSpan source) => new(source);

        public TimeSpan Value { get; private set; }

        private DateTimeSpan(TimeSpan timeSpan) => Value = timeSpan;

        public static DateTimeSpan FromMinutes(double value) => (DateTimeSpan)TimeSpan.FromMinutes(value);
        public static DateTimeSpan FromDays(double value) => FromFrequency(Frequency.Day, value);
        public static DateTimeSpan FromWeeks(double value) => FromFrequency(Frequency.Week, value);
        public static DateTimeSpan FromMonths(double value) => FromFrequency(Frequency.Month, value);
        public static DateTimeSpan FromYears(double value) => FromFrequency(Frequency.Year, value);
        public static DateTimeSpan FromFrequency(Frequency value) => value switch
        {
            Frequency.Day => FromFrequency(value, 1),
            Frequency.Week => FromFrequency(value, 7),
            Frequency.Month => FromFrequency(value, 1),
            Frequency.Year => FromFrequency(value, 1),
            _ => default
        };
        public static DateTimeSpan FromFrequency(Frequency frequency, double value)
        {
            if (frequency == Frequency.Unknown) return default;
            if (value <= 0) return default;

            var stamp = DateTime.UtcNow;

            return frequency switch
            {
                Frequency.Day => (DateTimeSpan)TimeSpan.FromDays(value),
                Frequency.Week => (DateTimeSpan)TimeSpan.FromDays(value),
                Frequency.Month => (DateTimeSpan)(stamp.AddMonths((int)value) - stamp),
                Frequency.Year => (DateTimeSpan)(stamp.AddYears((int)value) - stamp),
                _ => default
            };
        }

        public static DateTimeSpan Parse(string s) => Parse(s, null);
        public static DateTimeSpan Parse(string s, IFormatProvider provider) => TryParse(s, provider, out var value) ? value : default;

        public static bool TryParse([NotNullWhen(true)] string s, [MaybeNullWhen(false)] out DateTimeSpan result) => TryParse(s, null, out result);
        public static bool TryParse([NotNullWhen(true)] string s, IFormatProvider provider, [MaybeNullWhen(false)] out DateTimeSpan result)
        {
            result = default;

            if (RegexCompiled.IsMatch(s, TimeSpanSyntaxRegex))
            {
                TimeSpan value = default;

                bool parsed = provider is not null ? TimeSpan.TryParse(s, provider, out value) : TimeSpan.TryParse(s, out value);

                if (parsed)
                    result = (DateTimeSpan)value;
            }
            else if (s.TryParse<Frequency>(out var freqeuncy))
                result = FromFrequency(freqeuncy);
            else if (RegexCompiled.IsMatch(s, FrequencyCountSyntaxRegex))
            {
                var match = RegexCompiled.Match(s, FrequencyCountSyntaxRegex);
                var count = match.Groups[1].Value.Parse<double>();
                var frequency = match.Groups[2].Value.Parse<Frequency>();

                result = FromFrequency(freqeuncy, count);
            }

            return result != default;
        }
    }
}
