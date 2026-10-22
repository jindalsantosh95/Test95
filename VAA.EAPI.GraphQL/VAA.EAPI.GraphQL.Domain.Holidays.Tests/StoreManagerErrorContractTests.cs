using System.Reflection;
using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class StoreManagerErrorContractTests
{
    [Theory]
    [InlineData("BOOKING_NOT_FOUND", "Booking was not found.")]
    [InlineData("BOOKING_NOT_CANCELLABLE", "Booking cannot be cancelled.")]
    public void CancellationFailure_ShouldExposeStableGraphQlCode(string code, string message)
    {
        var httpException = new HttpRequestException("Conflict");
        httpException.Data["Response"] = $"[{{\"code\":\"{code}\",\"message\":\"{message}\"}}]";
        var method = typeof(StoreServiceProvider).GetMethod(
            "TryCreateStoreManagerGraphQlException",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var arguments = new object?[] { httpException, null };

        var translated = (bool)method.Invoke(null, arguments)!;
        var graphQlException = Assert.IsType<GraphQLException>(arguments[1]);
        var error = Assert.Single(graphQlException.Errors);

        Assert.True(translated);
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
    }

    [Fact]
    public void ClosureConflict_ShouldExposeStructuredGraphQlExtensions()
    {
        var httpException = new HttpRequestException("Conflict");
        httpException.Data["Response"] =
            """
            [
              {
                "code": "ACTIVE_BOOKINGS_EXIST",
                "message": "The store cannot be closed while active bookings exist.",
                "details": {
                  "storeId": "store-1",
                  "closureDate": "2026-07-29",
                  "activeBookingCount": 2
                }
              }
            ]
            """;
        var method = typeof(StoreServiceProvider).GetMethod(
            "TryCreateStoreManagerGraphQlException",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var arguments = new object?[] { httpException, null };

        var translated = (bool)method.Invoke(null, arguments)!;
        var graphQlException = Assert.IsType<GraphQLException>(arguments[1]);
        var error = Assert.Single(graphQlException.Errors);

        Assert.True(translated);
        Assert.Equal("ACTIVE_BOOKINGS_EXIST", error.Code);
        Assert.Equal("store-1", error.Extensions["storeId"]);
        Assert.Equal("2026-07-29", error.Extensions["closureDate"]);
        Assert.Equal(2L, error.Extensions["activeBookingCount"]);
    }

    [Fact]
    public void ScheduleConflict_ShouldExposeExactGraphQlResponseContract()
    {
        var httpException = new HttpRequestException("Conflict");
        httpException.Data["Response"] =
            """
            [
              {
                "code": "ACTIVE_BOOKINGS_EXIST",
                "message": "The requested change cannot be saved because it would affect active bookings.",
                "details": {
                  "storeId": "1025",
                  "rangeStart": "2026-07-24",
                  "rangeEndExclusive": "2027-07-24",
                  "activeBookingCount": 3,
                  "affectedDates": [
                    {
                      "date": "2026-07-27",
                      "activeBookingCount": 2
                    },
                    {
                      "date": "2026-08-03",
                      "activeBookingCount": 1
                    }
                  ],
                  "reason": "NO_MATCHING_SLOT_RULE"
                }
              }
            ]
            """;
        var method = typeof(StoreServiceProvider).GetMethod(
            "TryCreateStoreManagerGraphQlException",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var arguments = new object?[] { httpException, null };

        var translated = (bool)method.Invoke(null, arguments)!;
        var graphQlException = Assert.IsType<GraphQLException>(arguments[1]);
        var error = Assert.Single(graphQlException.Errors);

        Assert.True(translated);
        Assert.Equal("ACTIVE_BOOKINGS_EXIST", error.Code);
        Assert.Equal("The requested change cannot be saved because it would affect active bookings.", error.Message);

        var affectedDates = Assert.IsType<object[]>(error.Extensions!["affectedDates"]);
        var firstAffectedDate = Assert.IsType<Dictionary<string, object?>>(affectedDates[0]);
        var secondAffectedDate = Assert.IsType<Dictionary<string, object?>>(affectedDates[1]);

        Assert.Equal("2026-07-27", firstAffectedDate["date"]);
        Assert.Equal(2L, firstAffectedDate["activeBookingCount"]);
        Assert.Equal("2026-08-03", secondAffectedDate["date"]);
        Assert.Equal(1L, secondAffectedDate["activeBookingCount"]);

        var actualResponse = Newtonsoft.Json.Linq.JObject.FromObject(new
        {
            errors = new[]
            {
                new
                {
                    message = error.Message,
                    extensions = error.Extensions
                }
            }
        });
        var expectedResponse = Newtonsoft.Json.Linq.JObject.Parse(
            """
            {
              "errors": [
                {
                  "message": "The requested change cannot be saved because it would affect active bookings.",
                  "extensions": {
                    "code": "ACTIVE_BOOKINGS_EXIST",
                    "storeId": "1025",
                    "rangeStart": "2026-07-24",
                    "rangeEndExclusive": "2027-07-24",
                    "activeBookingCount": 3,
                    "affectedDates": [
                      {
                        "date": "2026-07-27",
                        "activeBookingCount": 2
                      },
                      {
                        "date": "2026-08-03",
                        "activeBookingCount": 1
                      }
                    ],
                    "reason": "NO_MATCHING_SLOT_RULE"
                  }
                }
              ]
            }
            """);

        Assert.True(Newtonsoft.Json.Linq.JToken.DeepEquals(expectedResponse, actualResponse));
    }
}
