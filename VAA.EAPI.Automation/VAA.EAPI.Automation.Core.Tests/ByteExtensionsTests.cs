using System.Text;
using VAA.EAPI.Automation.Core.Extensions;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests
{
    /// <summary>
    /// Behavior tests for the pure gzip helpers in <see cref="ByteExtensions"/>.
    /// See docs/HARNESS_ENGINEERING.md (Thread 4).
    /// </summary>
    public sealed class ByteExtensionsTests
    {
        [Theory]
        [InlineData("Harness engineering")]
        [InlineData("A longer payload, with punctuation: 1, 2, 3!")]
        public void Compress_ThenDecompress_RoundTripsTheOriginalBytes(string text)
        {
            var original = Encoding.UTF8.GetBytes(text);

            var roundTripped = original.Compress().Decompress();

            Assert.Equal(original, roundTripped);
        }

        [Fact]
        public void Compress_ThenDecompressToString_RoundTripsTheOriginalText()
        {
            const string text = "Harness engineering - Thread 4";

            var roundTripped = Encoding.UTF8.GetBytes(text).Compress().DecompressToString();

            Assert.Equal(text, roundTripped);
        }

        [Fact]
        public void Compress_Throws_ForNullInput()
        {
            byte[]? source = null;

            Assert.Throws<ArgumentNullException>(() => { source.Compress(); });
        }

        [Fact]
        public void Compress_Throws_ForEmptyInput() =>
            Assert.Throws<ArgumentNullException>(() => { Array.Empty<byte>().Compress(); });
    }
}
