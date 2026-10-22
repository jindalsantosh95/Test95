using VAA.EAPI.Automation.Core.Extensions;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests
{
    /// <summary>
    /// Behavior tests for the pure string helpers in <see cref="StringExtensions"/>.
    /// See docs/HARNESS_ENGINEERING.md (Thread 4).
    /// </summary>
    public sealed class StringExtensionsTests
    {
        [Theory]
        [InlineData("uat", "uat", true)]
        [InlineData("UAT", "uat", true)]    // case-insensitive
        [InlineData("uat", "prod", false)]
        [InlineData(null, null, true)]
        [InlineData("uat", null, false)]
        [InlineData(null, "uat", false)]
        public void Is_ComparesCaseInsensitively_AndHandlesNull(string? source, string? value, bool expected) =>
            Assert.Equal(expected, source.Is(value));

        [Theory]
        [InlineData("Standard Seat Window", "Standard Seat", true)]
        [InlineData("Standard Seat Window", "standard seat", true)]    // case-insensitive
        [InlineData("Standard Seat", "Upper", false)]
        public void Has_ChecksSubstringCaseInsensitively(string source, string value, bool expected) =>
            Assert.Equal(expected, source.Has(value));

        [Theory]
        [InlineData("uat", true)]
        [InlineData("UAT", true)]
        [InlineData("staging", false)]
        public void IsAny_MatchesAnyCandidateCaseInsensitively(string source, bool expected) =>
            Assert.Equal(expected, source.IsAny("dev", "uat", "prod"));

        [Theory]
        [InlineData("VSCL,VSUP,VSPR", "VSUP", true)]
        [InlineData("VSCL, VSUP , VSPR", " vsup ", true)]    // trims entries and value, case-insensitive
        [InlineData("VSCL,VSUP", "VSPR", false)]
        [InlineData("", "VSUP", false)]
        [InlineData("VSCL,VSUP", "", false)]
        public void ContainsCsvValue_MatchesTrimmedCaseInsensitiveEntry(string source, string value, bool expected) =>
            Assert.Equal(expected, source.ContainsCsvValue(value));

        [Theory]
        [InlineData("{0} of {1}", "{{0}} of {{1}}")]
        [InlineData("no braces", "no braces")]
        public void MakeInterpolationSafe_DoublesBraces(string source, string expected) =>
            Assert.Equal(expected, source.MakeInterpolationSafe());

        [Theory]
        [InlineData("a-b-c", true, "a_b_c")]
        [InlineData("a-b-c", false, "a-b-c")]
        public void ReplaceIf_ReplacesOnlyWhenConditionIsTrue(string source, bool condition, string expected) =>
            Assert.Equal(expected, source.ReplaceIf(condition, "-", "_"));

        [Theory]
        [InlineData("123", 123)]
        [InlineData("  42  ", 42)]    // trimmed before parsing
        public void TryParse_Int_ParsesValidInput(string source, int expected)
        {
            var parsed = source.TryParse(out int result);

            Assert.True(parsed);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12.3")]
        public void TryParse_Int_RejectsInvalidInput(string source) =>
            Assert.False(source.TryParse(out int _));

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("yes", true)]    // bespoke truthy tokens beyond bool.TryParse
        [InlineData("1", true)]
        [InlineData("on", true)]
        [InlineData("no", false)]
        public void TryParse_Bool_RecognisesBespokeTruthyTokens(string source, bool expected)
        {
            var parsed = source.TryParse(out bool result);

            Assert.True(parsed);    // bool parsing is total - every token resolves to true/false
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Monday", DayOfWeek.Monday)]
        [InlineData("friday", DayOfWeek.Friday)]    // case-insensitive
        public void TryParse_Enum_ParsesCaseInsensitively(string source, DayOfWeek expected)
        {
            var parsed = source.TryParse(out DayOfWeek result);

            Assert.True(parsed);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Parse_ThrowsFormatException_OnInvalidInput() =>
            Assert.Throws<FormatException>(() => { "not-a-number".Parse<int>(); });
    }
}
