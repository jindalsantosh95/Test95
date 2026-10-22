using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Http;
using Xunit;

namespace VAA.EAPI.GraphQL.Common.Tests
{
    public sealed class CompletionTrackingHttpContentTests
    {
        [Fact]
        public async Task FullRead_ReportsUtf8BytesOnceAndPreservesHeaders()
        {
            var counts = new List<long>();
            var payload = Encoding.UTF8.GetBytes("café");
            using var source = new ByteArrayContent(payload);
            source.Headers.TryAddWithoutValidation("X-Content-Test", "retained");
            using var content = new CompletionTrackingHttpContent(source, counts.Add);
            Assert.Equal("retained", Assert.Single(content.Headers.GetValues("X-Content-Test")));
            Assert.Equal("café", await content.ReadAsStringAsync());
            content.Dispose();
            Assert.Equal(new long[] { payload.Length }, counts);
        }

        [Fact]
        public async Task PartialRead_DisposalReportsOnlyConsumedBytesOnce()
        {
            var counts = new List<long>();
            using var source = new MemoryStream(Encoding.UTF8.GetBytes("response body"));
            using var content = new CompletionTrackingHttpContent(new StreamContent(source), counts.Add);
            await using (var stream = await content.ReadAsStreamAsync())
            {
                Assert.Equal(4, await stream.ReadAsync(new byte[4]));
                Assert.Empty(counts);
            }
            content.Dispose();
            Assert.Equal(new long[] { 4 }, counts);
            Assert.False(source.CanRead);
        }

        [Fact]
        public void UnreadDisposal_ReportsKnownLengthOnce()
        {
            var counts = new List<long>();
            using var content = new CompletionTrackingHttpContent(new ByteArrayContent(new byte[13]), counts.Add);
            content.Dispose();
            content.Dispose();
            Assert.Equal(new long[] { 13 }, counts);
        }
    }
}
