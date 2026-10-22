using System;
using HotChocolate;
using Xunit;

namespace VAA.EAPI.GraphQL.Tests;

public sealed class ErrorFilterTests
{
    [Theory]
    [InlineData("Unauthorized", "401", 401)]
    [InlineData("Forbidden", "403", 403)]
    public void AccessFailuresKeepSecurityStatusWithoutRequestConfiguration(string message, string code, int status)
    {
        var error = ErrorBuilder.New()
            .SetMessage("Unexpected Execution Error")
            .SetException(new UnauthorizedAccessException(message, new Exception("Sensitive internal detail")))
            .SetPath(Path.New("accountSummary"))
            .Build();

        var result = new ErrorFilter().OnError(error);

        Assert.Equal(message, result.Message);
        Assert.Equal(code, result.Code);
        Assert.Equal(status, result.Extensions!["httpStatusCode"]);
        Assert.Equal(error.Path, result.Path);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void ExistingGraphQLErrorIsPreserved()
    {
        var error = ErrorBuilder.New().SetMessage("Invalid input").SetCode("400").Build();

        Assert.Same(error, new ErrorFilter().OnError(error));
    }
}
