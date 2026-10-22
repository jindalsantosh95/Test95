using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Holidays.Constants;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.AppointmentBooking;
using VAA.EAPI.GraphQL.Domain.Holidays.Mutations;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class SaveAppointmentBookingMutationTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public SaveAppointmentBookingMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        Setup.CreateTestEnvironmentSettings();

        _serviceProvider = new ServiceCollection().AddLogging()
                                                  .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                  .AddSingleton<IHttpConnector, HttpConnector>()
                                                  .AddSingleton<IAppointmentBookingServiceProvider, StoreServiceProvider>()
                                                  .AddSingleton<SaveAppointmentBookingMutation>()
                                                  .BuildServiceProvider();
    }

    [Fact]
    public void Constructor_ShouldThrowNullException_WhenStoreServiceProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SaveAppointmentBookingMutation(null!));
    }

    [Fact]
    public async Task Mutation_ShouldThrowNullException_WhenRequestIsNull()
    {
        var _mutation = _serviceProvider.GetService<SaveAppointmentBookingMutation>();
        await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.SaveAppointmentBooking(null!));
    }

    [Fact]
    public async Task Mutation_ShouldReturnAppointmentBooking_ForGoodRequest()
    {
        var request = new AppointmentBookingRequest
        {
            AppointmentType = AppointmentType.InStore,
            AppointmentReason = "Travel advice",
            BookingReference = "ABC123",
            Date = new DateOnly(2026, 7, 16),
            SlotStart = DateTimeOffset.Parse("2026-07-16T09:00:00+01:00"),
            SlotEnd = DateTimeOffset.Parse("2026-07-16T09:30:00+01:00"),
            LocationId = "LON001",
            VisitPurpose = "Holiday planning",
            Customer = new BookingCustomer
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                Phone = "+447700900123"
            }
        };

        var _mutation = _serviceProvider.GetService<SaveAppointmentBookingMutation>();
        var result = await _mutation.SaveAppointmentBooking(request);

        Assert.NotNull(result);
        Assert.Equal("APT-12345", result.AppointmentReferenceId);
        Assert.Equal(new DateOnly(2026, 7, 16), result.Date);
        Assert.Equal("London Heathrow Store", result.Store);
        Assert.True(result.SlotDetails.IsAvailable);
    }
}
