using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests
{

    public class StoreAppointmentHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public StoreAppointmentHandlerTests()
        {
            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<FakeStoreAppointmentRepository>()
                                                      .AddSingleton<IStoreAppointmentRepository>(provider => provider.GetRequiredService<FakeStoreAppointmentRepository>())
                                                      .AddSingleton<FakeStoreRepository>()
                                                      .AddSingleton<IStoreRepository>(provider => provider.GetRequiredService<FakeStoreRepository>())
                                                      .AddSingleton<FakeStoreAppointmentCrmPublisher>()
                                                      .AddSingleton<IStoreAppointmentEmailPublisher>(provider => provider.GetRequiredService<FakeStoreAppointmentCrmPublisher>())
                                                      .AddSingleton<StoreAppointmentHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task HandleAsync_ShouldPersistNonPiiDataAndPublishCrmEvent_WhenRequestIsValid()
        {
            var storeAppointmentRepository = _serviceProvider.GetRequiredService<FakeStoreAppointmentRepository>();
            var storeAppointmentCrmPublisher = _serviceProvider.GetRequiredService<FakeStoreAppointmentCrmPublisher>();
            var storeAppointmentHandler = _serviceProvider.GetRequiredService<StoreAppointmentHandler>();

            storeAppointmentRepository.SaveResult = new StoreAppointmentSaveResult { IsSuccess = true };

            var response = (await storeAppointmentHandler.SaveAppointmentAsync(BuildValidRequest())).Value!.Value;

            Assert.NotNull(storeAppointmentRepository.SavedAppointment);
            Assert.Equal("InStore", storeAppointmentRepository.SavedAppointment!.AppointmentType.ToString());
            Assert.Equal("Reason", storeAppointmentRepository.SavedAppointment.AppointmentReason);
            Assert.Equal("LOC001", storeAppointmentRepository.SavedAppointment.LocationId);
            Assert.Equal("APT details", storeAppointmentRepository.SavedAppointment.AdditionalDetails);
            Assert.NotNull(storeAppointmentCrmPublisher.PublishedRequest);
            Assert.Equal("Ben", storeAppointmentCrmPublisher.PublishedRequest!.Customer!.FirstName);
            Assert.Equal("Store Name", response.Store);
            Assert.StartsWith("BR-LO-", response.AppointmentReferenceId);
            Assert.Equal(response.AppointmentReferenceId, storeAppointmentRepository.SavedAppointment.Id);
            Assert.Equal(response.AppointmentReferenceId, storeAppointmentCrmPublisher.PublishedAppointmentId);
            Assert.Equal(new TimeOnly(9, 0), storeAppointmentRepository.SavedAppointment.SlotStart);
            Assert.Equal(new TimeOnly(10, 0), storeAppointmentRepository.SavedAppointment.SlotEnd);
            Assert.Equal(TimeSpan.Zero, response.SlotDetails.StartTime.Offset);
            Assert.Equal(TimeSpan.Zero, response.SlotDetails.EndTime.Offset);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnBritishSummerTimeOffset_ForSummerDateSlots()
        {
            var storeAppointmentRepository = _serviceProvider.GetRequiredService<FakeStoreAppointmentRepository>();
            var storeAppointmentHandler = _serviceProvider.GetRequiredService<StoreAppointmentHandler>();

            storeAppointmentRepository.SaveResult = new StoreAppointmentSaveResult { IsSuccess = true };
            var request = BuildValidRequest();
            request.Date = new DateOnly(2026, 7, 16);
            request.SlotStart = new DateTimeOffset(2026, 7, 16, 9, 0, 0, TimeSpan.Zero);
            request.SlotEnd = new DateTimeOffset(2026, 7, 16, 10, 0, 0, TimeSpan.Zero);

            var response = (await storeAppointmentHandler.SaveAppointmentAsync(request)).Value!.Value;

            Assert.Equal(TimeSpan.FromHours(1), response.SlotDetails.StartTime.Offset);
            Assert.Equal("2026-07-16T10:00:00+01:00", response.SlotDetails.StartTime.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        }

        [Fact]
        public async Task HandleAsync_ShouldConvertUtcSlotInput_ToUkLocalTimeForSaveAndEmail()
        {
            var storeAppointmentRepository = _serviceProvider.GetRequiredService<FakeStoreAppointmentRepository>();
            var storeAppointmentHandler = _serviceProvider.GetRequiredService<StoreAppointmentHandler>();
            var storeAppointmentCrmPublisher = _serviceProvider.GetRequiredService<FakeStoreAppointmentCrmPublisher>();

            storeAppointmentRepository.SaveResult = new StoreAppointmentSaveResult { IsSuccess = true };
            var request = BuildValidRequest();
            request.Date = new DateOnly(2026, 7, 16);
            request.SlotStart = new DateTimeOffset(2026, 7, 16, 9, 0, 0, TimeSpan.Zero);
            request.SlotEnd = new DateTimeOffset(2026, 7, 16, 10, 0, 0, TimeSpan.Zero);

            var response = (await storeAppointmentHandler.SaveAppointmentAsync(request)).Value!.Value;

            Assert.Equal(new TimeOnly(10, 0), storeAppointmentRepository.SavedAppointment!.SlotStart);
            Assert.Equal(new TimeOnly(11, 0), storeAppointmentRepository.SavedAppointment!.SlotEnd);
            Assert.Equal(new DateTimeOffset(2026, 7, 16, 10, 0, 0, TimeSpan.FromHours(1)), storeAppointmentCrmPublisher.PublishedRequest!.SlotStart);
            Assert.Equal(new DateTimeOffset(2026, 7, 16, 10, 0, 0, TimeSpan.FromHours(1)), response.SlotDetails.StartTime);
        }
        [Fact]
        public async Task HandleAsync_ShouldThrowArgumentException_WhenLocationIdIsMissing()
        {
            var storeAppointmentRepository = _serviceProvider.GetRequiredService<FakeStoreAppointmentRepository>();
            var storeAppointmentHandler = _serviceProvider.GetRequiredService<StoreAppointmentHandler>();

            storeAppointmentRepository.SaveResult = new StoreAppointmentSaveResult { IsSuccess = true };
            var request = BuildValidRequest();
            request.LocationId = string.Empty;

            await Assert.ThrowsAsync<ArgumentException>(() => storeAppointmentHandler.SaveAppointmentAsync(request));
        }

        [Fact]
        public async Task HandleAsync_ShouldThrowArgumentException_WhenRequestDateDoesNotMatchUkLocalSlotDate()
        {
            var storeAppointmentRepository = _serviceProvider.GetRequiredService<FakeStoreAppointmentRepository>();
            var storeAppointmentHandler = _serviceProvider.GetRequiredService<StoreAppointmentHandler>();

            storeAppointmentRepository.SaveResult = new StoreAppointmentSaveResult { IsSuccess = true };
            var request = BuildValidRequest();
            request.Date = new DateOnly(2026, 7, 16);
            request.SlotStart = new DateTimeOffset(2026, 7, 16, 23, 30, 0, TimeSpan.Zero);
            request.SlotEnd = new DateTimeOffset(2026, 7, 17, 0, 30, 0, TimeSpan.Zero);

            await Assert.ThrowsAsync<ArgumentException>(() => storeAppointmentHandler.SaveAppointmentAsync(request));
        }

        [Fact]
        public async Task HandleAsync_ShouldIncrementAppointmentReferenceId_WhenLatestExists()
        {
            var storeAppointmentRepository = _serviceProvider.GetRequiredService<FakeStoreAppointmentRepository>();
            var storeAppointmentHandler = _serviceProvider.GetRequiredService<StoreAppointmentHandler>();

            storeAppointmentRepository.SaveResult = new StoreAppointmentSaveResult { IsSuccess = true };
            storeAppointmentRepository.LatestAppointmentReferenceId = "BR-LO-000009";

            var response = (await storeAppointmentHandler.SaveAppointmentAsync(BuildValidRequest())).Value!.Value;

            Assert.Equal("BR-LO-000010", response.AppointmentReferenceId);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnBadRequestResponse_WhenSlotIsNotAvailable()
        {
            var storeAppointmentRepository = _serviceProvider.GetRequiredService<FakeStoreAppointmentRepository>();
            var storeAppointmentHandler = _serviceProvider.GetRequiredService<StoreAppointmentHandler>();
            var storeAppointmentCrmPublisher = _serviceProvider.GetRequiredService<FakeStoreAppointmentCrmPublisher>();

            storeAppointmentRepository.SaveResult = new StoreAppointmentSaveResult
            {
                IsSuccess = false,
                ErrorCode = "SLOT_FULL",
                ErrorMessage = "Requested slot is no longer available."
            };

            var result = await storeAppointmentHandler.SaveAppointmentAsync(BuildValidRequest());

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("SLOT_FULL", result.Value.Errors[0].Code);
            Assert.Null(storeAppointmentRepository.SavedAppointment);
            Assert.Null(storeAppointmentCrmPublisher.PublishedAppointmentId);
        }

        private static AppointmentBookingRequest BuildValidRequest()
        {
            return new AppointmentBookingRequest
            {
                AppointmentType = AppointmentType.InStore,
                AppointmentReason = "Reason",
                BookingReference = "BOOK01",
                Date = new DateOnly(2026, 12, 16),
                SlotStart = new DateTimeOffset(2026, 12, 16, 9, 0, 0, TimeSpan.Zero),
                SlotEnd = new DateTimeOffset(2026, 12, 16, 10, 0, 0, TimeSpan.Zero),
                LocationId = "LOC001",
                VisitPurpose = "Holiday",
                VisitPurposeDetails = "Beach",
                AdditionalDetails = "APT details",
                Customer = new BookingCustomer
                {
                    FirstName = "Ben",
                    LastName = "Smith",
                    Email = "customer@test.com",
                    Phone = "012345"
                }
            };
        }

        private sealed class FakeStoreAppointmentCrmPublisher : IStoreAppointmentEmailPublisher
        {
            public AppointmentBookingRequest? PublishedRequest { get; private set; }
            public string? PublishedAppointmentId { get; private set; }

            public Task PublishAsync(string appointmentId, AppointmentBookingRequest request, StoreResult? store)
            {
                PublishedRequest = request;
                PublishedAppointmentId = appointmentId;
                return Task.CompletedTask;
            }
        }

        private sealed class FakeStoreRepository : IStoreRepository
        {
            public Task<IReadOnlyCollection<StoreResult>> GetStoresAsync() => Task.FromResult<IReadOnlyCollection<StoreResult>>(Array.Empty<StoreResult>());

            public Task<StoreResult?> GetStoreByIdAsync(string storeId)
            {
                return Task.FromResult<StoreResult?>(new StoreResult
                {
                    StoreId = storeId,
                    Name = "Store Name",
                    Label = "Store Label"
                });
            }

            public Task<IReadOnlyCollection<StoreSlot>> GetStoreSlotsByDateAsync(string storeId, DateTime selectedDate) => Task.FromResult<IReadOnlyCollection<StoreSlot>>(Array.Empty<StoreSlot>());

            public Task<IReadOnlyCollection<StoreDayAvailability>> GetStoreCalendarAvailabilityAsync(string storeId, int year, int month)
                => Task.FromResult<IReadOnlyCollection<StoreDayAvailability>>(Array.Empty<StoreDayAvailability>());
        }

        private sealed class FakeStoreAppointmentRepository : IStoreAppointmentRepository
        {
            public Appointment? SavedAppointment { get; private set; }
            public string? LatestAppointmentReferenceId { get; set; }
            public StoreAppointmentSaveResult SaveResult { get; set; } = new() { IsSuccess = true };

            public Task<string?> GetLatestAppointmentReferenceIdAsync(string storeInitials)
            {
                return Task.FromResult(LatestAppointmentReferenceId);
            }

            public Task<StoreAppointmentSaveResult> AddAsync(Appointment appointment)
            {
                if (SaveResult.IsSuccess)
                {
                    SavedAppointment = appointment;
                }

                return Task.FromResult(SaveResult);
            }

        }
    }
}
