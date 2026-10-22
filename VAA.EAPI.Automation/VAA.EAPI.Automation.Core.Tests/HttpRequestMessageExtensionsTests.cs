using System.Text;
using VAA.EAPI.Automation.Core.Extensions;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests
{
    public sealed class HttpRequestMessageExtensionsTests
    {
        [Theory]
        [InlineData("complete body", 0, "complete body")]
        [InlineData("prefix body", 7, "body")]
        [InlineData("", 0, "")]
        public void BuildKey_ReadsRemainingBodyFromNonSeekableStream(string body, int position, string expected)
        {
            using ShortReadStream stream = new(Encoding.UTF8.GetBytes(body), position);
            using HttpRequestMessage request = new(HttpMethod.Post, "https://example.test/request")
            {
                Content = new StreamContent(stream)
            };

            string key = request.BuildKey();

            Assert.EndsWith(expected + Environment.NewLine, key);
            Assert.DoesNotContain('\0', key);
            Assert.True(stream.CanRead);
        }

        private sealed class ShortReadStream : Stream
        {
            private readonly MemoryStream source;

            public ShortReadStream(byte[] bytes, int position)
            {
                source = new MemoryStream(bytes);
                source.Position = position;
            }

            public override bool CanRead => source.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count) => source.Read(buffer, offset, Math.Min(count, 2));
            public override void Flush() => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    source.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}
