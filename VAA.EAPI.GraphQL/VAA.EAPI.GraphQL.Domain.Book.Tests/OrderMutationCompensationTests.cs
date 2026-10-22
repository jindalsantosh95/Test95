using System.Net;
using System.Reflection;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Book.Mutations;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests;

public sealed class OrderMutationCompensationTests
{
    [Theory]
    [InlineData(HttpStatusCode.BadRequest, true)]
    [InlineData(HttpStatusCode.UnprocessableEntity, true)]
    [InlineData(HttpStatusCode.RequestTimeout, false)]
    [InlineData(HttpStatusCode.TooManyRequests, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsConfirmedOrderRejection_OnlyReturnsTrueForTerminalOrderClientErrors(HttpStatusCode statusCode, bool expected)
    {
        using var response = new HttpResponseMessage(statusCode)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://order.example/order/response")
        };
        var exception = new HttpRequestException("Order request failed.", new HttpResponseException(response), statusCode);
        var method = typeof(OrderMutation).GetMethod("IsConfirmedOrderRejection", BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = (bool)method.Invoke(null, [exception])!;

        Assert.Equal(expected, result);
    }
}
