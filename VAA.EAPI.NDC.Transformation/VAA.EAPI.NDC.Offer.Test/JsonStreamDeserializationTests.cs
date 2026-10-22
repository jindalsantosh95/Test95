using System.Text;
using VAA.EAPI.Common.Serialisation;
using Xunit;

namespace VAA.EAPI.NDC.Offer.Tests;

public sealed class JsonStreamDeserializationTests
{
    [Fact]
    public async Task DeserializeAsync_ReadsJsonDirectlyFromStream()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"value\":\"streamed\"}"));

        var result = await Json.DeserializeAsync<TestResponse>(stream, TestContext.Current.CancellationToken);

        Assert.Equal("streamed", result.Value);
    }

    [Fact]
    public async Task DeserializeAsync_ThrowsWhenStreamIsEmpty()
    {
        using var stream = new MemoryStream();

        await Assert.ThrowsAsync<ArgumentNullException>(() => Json.DeserializeAsync<TestResponse>(stream, TestContext.Current.CancellationToken));
    }

    private sealed class TestResponse
    {
        public string Value { get; set; } = string.Empty;
    }
}
