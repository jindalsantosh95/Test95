using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using VAA.EAPI.GraphQL.Domain.Holidays.Mutations;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class ReplaceSlotRulesMutationTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public ReplaceSlotRulesMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        Setup.CreateTestEnvironmentSettings();

        _serviceProvider = new ServiceCollection().AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IStoreManagerServiceProvider, StoreServiceProvider>()
            .AddSingleton<ReplaceSlotRulesMutation>()
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData(3, "19:30", "20:00", 60, 1, "dayOfWeek 3")]
    [InlineData(1, "9:00", "17:00", 60, 1, "dayOfWeek 1")]
    [InlineData(1, "17:00", "09:00", 60, 1, "endTime must be after startTime")]
    [InlineData(7, "09:00", "17:00", 60, 1, "dayOfWeek must be between 0 and 6")]
    [InlineData(1, "09:00", "17:00", 0, 1, "slotDurationMinutes must be greater than zero")]
    [InlineData(1, "09:00", "17:00", 60, 0, "slotCapacity must be greater than zero")]
    public async Task ReplaceSlotRules_WithInvalidRule_ThrowsRequestValidationException(
        int dayOfWeek,
        string startTime,
        string endTime,
        int duration,
        int capacity,
        string expectedMessage)
    {
        var request = BuildRequest(dayOfWeek, startTime, endTime, duration, capacity);

        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => GetMutation().ReplaceSlotRules("1043", request));

        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task ReplaceSlotRules_WithNullRule_ThrowsRequestValidationException()
    {
        var request = new SlotRuleReplaceRequest
        {
            Rules = [null!]
        };

        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => GetMutation().ReplaceSlotRules("1043", request));

        Assert.Contains("rule must not be null", exception.Message);
    }

    [Fact]
    public async Task ReplaceSlotRules_WithValidWindow_CallsServiceProvider()
    {
        var response = new[]
        {
            new SlotRuleResponse
            {
                Id = "1043-1-0001",
                DayOfWeek = 1,
                StartTime = "09:00",
                EndTime = "17:00",
                SlotDurationMinutes = 60,
                SlotCapacity = 1
            }
        };
        var request = BuildRequest(1, "09:00", "17:00", 60, 1);
        var result = await GetMutation().ReplaceSlotRules("1043", request);

        var rule = Assert.Single(result);
        Assert.Equal(response[0].Id, rule.Id);
        Assert.Equal(response[0].SlotDurationMinutes, rule.SlotDurationMinutes);
        Assert.Equal(response[0].SlotCapacity, rule.SlotCapacity);
    }

    private ReplaceSlotRulesMutation GetMutation() =>
        _serviceProvider.GetRequiredService<ReplaceSlotRulesMutation>();

    private static SlotRuleReplaceRequest BuildRequest(
        int dayOfWeek,
        string startTime,
        string endTime,
        int duration,
        int capacity)
    {
        return new SlotRuleReplaceRequest
        {
            Rules =
            [
                new SlotRuleRequest
                {
                    DayOfWeek = dayOfWeek,
                    StartTime = startTime,
                    EndTime = endTime,
                    SlotDurationMinutes = duration,
                    SlotCapacity = capacity
                }
            ]
        };
    }
}
