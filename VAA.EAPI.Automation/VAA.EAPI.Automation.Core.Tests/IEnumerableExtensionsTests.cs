using VAA.EAPI.Automation.Core.Extensions;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests
{
    /// <summary>
    /// Behavior tests for the pure collection helpers in
    /// <see cref="IEnumerableExtensions"/>. See docs/HARNESS_ENGINEERING.md (Thread 4).
    /// </summary>
    public sealed class IEnumerableExtensionsTests
    {
        [Fact]
        public void IsNullOrEmpty_IsTrue_ForNull()
        {
            int[]? source = null;

            Assert.True(source.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_IsTrue_ForEmpty() =>
            Assert.True(Array.Empty<int>().IsNullOrEmpty());

        [Fact]
        public void IsNullOrEmpty_IsFalse_ForPopulated() =>
            Assert.False(new[] { 1 }.IsNullOrEmpty());

        [Fact]
        public void IfIsNullOrEmpty_ReturnsDefault_ForNull()
        {
            int[]? source = null;

            Assert.Equal(new[] { 9 }, source.IfIsNullOrEmpty(9));
        }

        [Fact]
        public void IfIsNullOrEmpty_ReturnsDefault_ForEmpty() =>
            Assert.Equal(new[] { 7, 8 }, Array.Empty<int>().IfIsNullOrEmpty(7, 8));

        [Fact]
        public void IfIsNullOrEmpty_ReturnsSource_WhenPopulated() =>
            Assert.Equal(new[] { 1, 2 }, new[] { 1, 2 }.IfIsNullOrEmpty(9));

        [Fact]
        public void ConvertToCsv_JoinsItemsWithNewLines()
        {
            var csv = new[] { 1, 2, 3 }.ConvertToCsv();

            var newLine = Environment.NewLine;
            Assert.Equal($"1{newLine}2{newLine}3{newLine}", csv);
        }

        [Fact]
        public void ConvertToCsv_PrependsHeaderLine_WhenSupplied()
        {
            var csv = new[] { 1, 2 }.ConvertToCsv("Id");

            var newLine = Environment.NewLine;
            Assert.Equal($"Id{newLine}1{newLine}2{newLine}", csv);
        }
    }
}
