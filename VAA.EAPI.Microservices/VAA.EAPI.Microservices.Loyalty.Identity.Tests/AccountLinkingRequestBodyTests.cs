using System.Reflection;
using System.Text;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

public sealed class AccountLinkingRequestBodyTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task ReadsSmallProofBodyButStopsBeforeConsumingOversizedBody(bool oversized) =>
        AssertBodyAsync<CreateLinkingProofRequest>(oversized, "account-linking-proof",
            "{\"transactionId\":\"test\"}", value => value.TransactionId);

    private static async Task AssertBodyAsync<T>(bool oversized, string route, string validBody, Func<T, string> value) where T : class
    {
        var body = oversized ? "{\"padding\":\"" + new string('x', 131072) + "\"}" : validBody;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        var request = new TestHttpRequestData(new TestFunctionContext(), new Uri("https://identity.test/" + route), stream, "POST");
        var read = typeof(AccountLinkingController).GetMethod("ReadAsync", BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeof(T));
        var result = await (Task<T>)read.Invoke(null, new object[] { request, TestContext.Current.CancellationToken })!;
        if (oversized)
        {
            Assert.Null(result);
            Assert.True(request.Body.Position < request.Body.Length);
        }
        else Assert.Equal("test", value(result));
    }
}
