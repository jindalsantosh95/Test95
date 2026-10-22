
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure.Core.Serialization;
using VAA.EAPI.Core.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Triggers.Http;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests;

public class StoreManagerControllerValidationTests
{
    private readonly IServiceProvider _serviceProvider;

    public StoreManagerControllerValidationTests()
    {
        _serviceProvider = new ServiceCollection().AddLogging()
                                                  .AddSingleton<FakeStoreManagerService>()
                                                  .AddSingleton<IStoreManagerService>(provider => provider.GetRequiredService<FakeStoreManagerService>())
                                                  .AddSingleton<StoreAdminController>()
                                                  .AddSingleton<StoreManagerController>()
                                                  .BuildServiceProvider();
    }

    [Fact]
    public async Task GetAdminStore_ShouldReturnBadRequest_WhenIdIsMissing()
    {
        var sut = _serviceProvider.GetRequiredService<StoreAdminController>();
        var response = await sut.GetAdminStore(CreateRequest(), string.Empty);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminStore_ShouldReturnNotFound_WhenStoreDoesNotExist()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.GetStoreResult = ServiceResult<StoreResponse>.NotFound("Store not found.");
        var sut = _serviceProvider.GetRequiredService<StoreAdminController>();

        var response = await sut.GetAdminStore(CreateRequest(), "store-1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStore_ShouldReturnNotFound_WhenStoreDoesNotExist()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.GetStoreResult = ServiceResult<StoreResponse>.NotFound("Store not found.");
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.GetStore(CreateRequest(), "store-1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStore_ShouldReturnNotFound_WhenStoreDoesNotExist()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.UpdateStoreResult = ServiceResult<StoreResponse>.NotFound("Store not found.");
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.UpdateStore(CreateRequest(), "store-1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAdminStore_ShouldReturnNotFound_WhenStoreDoesNotExist()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.UpdateStoreResult = ServiceResult<StoreResponse>.NotFound("Store not found.");
        var sut = _serviceProvider.GetRequiredService<StoreAdminController>();

        var response = await sut.UpdateAdminStore(CreateRequest(), "store-1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStore_ShouldReturnStructuredConflict_WhenClosingWouldAffectBookings()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.UpdateStoreResult = ActiveBookingConflict<StoreResponse>("STORE_MARKED_CLOSED");
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.UpdateStore(CreateRequest(body: """{"isClosed":true}"""), "store-1");
        var body = await ReadBody(response);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("\"Code\":\"ACTIVE_BOOKINGS_EXIST\"", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"reason\":\"STORE_MARKED_CLOSED\"", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReplaceOpeningHours_ShouldReturnStructuredConflict_WhenBookingsFallOutsideHours()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.ReplaceOpeningHoursResult =
            ActiveBookingConflict<IReadOnlyCollection<OpeningHoursEntryResponse>>("OUTSIDE_PROPOSED_OPENING_HOURS");
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.ReplaceOpeningHours(
            CreateRequest(body: """{"openingHours":[{"dayOfWeek":1,"opensAt":"10:00","closesAt":"16:00"}]}"""),
            "store-1");
        var body = await ReadBody(response);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("\"reason\":\"OUTSIDE_PROPOSED_OPENING_HOURS\"", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReplaceSlotRules_ShouldReturnStructuredConflict_WhenCapacityIsReduced()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.ReplaceSlotRulesResult =
            ActiveBookingConflict<IReadOnlyCollection<SlotRuleResponse>>("CAPACITY_REDUCTION");
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.ReplaceSlotRules(
            CreateRequest(body: """{"rules":[{"dayOfWeek":1,"startTime":"09:00","endTime":"17:00","slotDurationMinutes":30,"slotCapacity":1}]}"""),
            "store-1");
        var body = await ReadBody(response);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("\"reason\":\"CAPACITY_REDUCTION\"", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetBookings_ShouldReturnBadRequest_WhenDateQueryMissing()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();
        var response = await sut.GetBookings(CreateRequest(), "store-1");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWindows_ShouldPreservePositiveUtcOffsets_InQueryParameters()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.GetWindows(
            CreateRequest("?from=2026-06-11T09:00:00+01:00&to=2026-06-11T10:00:00+01:00"),
            "store-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new DateTimeOffset(2026, 6, 11, 9, 0, 0, TimeSpan.FromHours(1)), service.GetWindowsFrom);
        Assert.Equal(new DateTimeOffset(2026, 6, 11, 10, 0, 0, TimeSpan.FromHours(1)), service.GetWindowsTo);
    }

    [Fact]
    public async Task GenerateSlots_ShouldReturnCreated_WhenSlotsAreGenerated()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.GenerateSlots(CreateRequest(), "store-1");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateClosure_ShouldReturnCreated_WhenClosureIsCreated()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.CreateClosure(CreateRequest(), "store-1");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSlotStatus_ShouldReturnNotFound_WhenSlotDoesNotExist()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.UpdateSlotStatusResult = ServiceResult<WindowResponse>.NotFound("Slot not found.");
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.UpdateSlotStatus(CreateRequest(), "store-1", "slot-1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSlotAllocations_ShouldReturnBadRequest_WhenStartAtQueryMissing()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.GetSlotAllocations(CreateRequest(), "store-1", "slot-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSlotAllocations_ShouldPreservePositiveUtcOffset_InStartAtQuery()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.GetSlotAllocations(
            CreateRequest("?startAt=2026-07-15T09:00:00+01:00"),
            "store-1",
            "slot-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("store-1", service.GetSlotAllocationsStoreId);
        Assert.Equal("slot-1", service.GetSlotAllocationsSlotId);
        Assert.Equal(
            new DateTimeOffset(2026, 7, 15, 9, 0, 0, TimeSpan.FromHours(1)),
            service.GetSlotAllocationsStartAt);
    }

    [Fact]
    public async Task UpdateSlotAllocationStatus_ShouldReturnBadRequest_WhenAllocationIdIsMissing()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.UpdateSlotAllocationStatus(
            CreateRequest(),
            "store-1",
            "slot-1",
            string.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSlotAllocationStatus_ShouldReturnBadRequest_WhenBodyIsInvalidJson()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.UpdateSlotAllocationStatus(
            CreateRequest(body: "{"),
            "store-1",
            "slot-1",
            "allocation-20260715-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSlotAllocationStatus_ShouldReturnBadRequest_WhenBodyIsNull()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.UpdateSlotAllocationStatus(
            CreateRequest(body: "null"),
            "store-1",
            "slot-1",
            "allocation-20260715-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSlotAllocationStatus_ShouldReturnBadRequest_WhenBodyIsWhitespace()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.UpdateSlotAllocationStatus(
            CreateRequest(body: "   "),
            "store-1",
            "slot-1",
            "allocation-20260715-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSlotAllocationStatus_ShouldPassRequestToService()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.UpdateSlotAllocationStatus(
            CreateRequest(body: """{"status":"BLOCKED","reason":"Staff unavailable"}"""),
            "store-1",
            "slot-1",
            "allocation-20260715-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("store-1", service.UpdateSlotAllocationStatusStoreId);
        Assert.Equal("slot-1", service.UpdateSlotAllocationStatusSlotId);
        Assert.Equal("allocation-20260715-1", service.UpdateSlotAllocationStatusAllocationId);
        Assert.Equal(SlotStatus.Blocked, service.UpdateSlotAllocationStatusRequest?.Status);
        Assert.Equal("Staff unavailable", service.UpdateSlotAllocationStatusRequest?.Reason);
    }

    [Fact]
    public async Task CreateClosure_ShouldReturnConflict_WhenServiceReturnsConflict()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.CreateClosureResult = ServiceResult<StoreClosureResponse>.Conflict("Closure already exists.");
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.CreateClosure(CreateRequest(), "store-1");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateClosure_ShouldReturnStructuredActiveBookingConflict()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.CreateClosureResult = ServiceResult<StoreClosureResponse>.Conflict(
            "ACTIVE_BOOKINGS_EXIST",
            "The store cannot be closed while active bookings exist.",
            new Dictionary<string, object?>
            {
                ["storeId"] = "store-1",
                ["closureDate"] = "2026-07-29",
                ["activeBookingCount"] = 2
            });
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.CreateClosure(
            CreateRequest(body: """{"closureDate":"2026-07-29","closureType":"FULL_DAY"}"""),
            "store-1");
        response.Body.Position = 0;
        var body = await new StreamReader(response.Body).ReadToEndAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("\"Code\":\"ACTIVE_BOOKINGS_EXIST\"", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"activeBookingCount\":2", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelBooking_ShouldPassAuditRequestToService()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.CancelBooking(
            CreateRequest(body: """{"reason":"STORE_CLOSURE","customerContactConfirmed":true,"cancelledByUserId":"user-1","cancelledByDisplayName":"Manager"}"""),
            "store-1",
            "BR-10-000171");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("store-1", service.CancelBookingStoreId);
        Assert.Equal("BR-10-000171", service.CancelBookingId);
        Assert.True(service.CancelBookingRequest?.CustomerContactConfirmed);
        Assert.Equal("user-1", service.CancelBookingRequest?.CancelledByUserId);
    }

    [Fact]
    public async Task CancelBooking_ShouldReturnBadRequest_WhenBodyIsMalformedJson()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.CancelBooking(
            CreateRequest(body: "{"),
            "store-1",
            "BR-10-000171");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClosure_ShouldReturnNoContent_WhenDeleteSucceeds()
    {
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.DeleteClosure(CreateRequest(), "store-1", "closure-1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, response.Body.Length);
    }

    [Fact]
    public async Task DeleteClosure_ShouldReturnConflict_WhenServiceReturnsConflict()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.DeleteClosureResult = ServiceResult<bool>.Conflict("Closure has bookings.");
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.DeleteClosure(CreateRequest(), "store-1", "closure-1");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClosure_ShouldReturnNotFound_WhenServiceReturnsNotFound()
    {
        var service = _serviceProvider.GetRequiredService<FakeStoreManagerService>();
        service.DeleteClosureResult = ServiceResult<bool>.NotFound("Closure not found.");
        var sut = _serviceProvider.GetRequiredService<StoreManagerController>();

        var response = await sut.DeleteClosure(CreateRequest(), "store-1", "closure-1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static HttpRequestData CreateRequest(string query = "", string body = "{}")
    {
        var serviceProvider = new ServiceCollection()
            .AddOptions()
            .Configure<WorkerOptions>(options => options.Serializer = new JsonObjectSerializer())
            .BuildServiceProvider();

        var context = new TestFunctionContext();
        context.InstanceServices = serviceProvider;

        return new TestHttpRequestData(
            context,
            new Uri($"https://localhost/test{query}"),
            new MemoryStream(Encoding.UTF8.GetBytes(body)));
    }

    private static ServiceResult<T> ActiveBookingConflict<T>(string reason) =>
        ServiceResult<T>.Conflict(
            "ACTIVE_BOOKINGS_EXIST",
            "The requested change cannot be saved because it would affect active bookings.",
            new Dictionary<string, object?>
            {
                ["storeId"] = "store-1",
                ["rangeStart"] = "2026-07-24",
                ["rangeEndExclusive"] = "2027-07-24",
                ["activeBookingCount"] = 2,
                ["affectedDates"] = new[]
                {
                    new Dictionary<string, object?>
                    {
                        ["date"] = "2026-07-27",
                        ["activeBookingCount"] = 2
                    }
                },
                ["reason"] = reason
            });

    private static async Task<string> ReadBody(HttpResponseData response)
    {
        response.Body.Position = 0;
        return await new StreamReader(response.Body).ReadToEndAsync();
    }

    private sealed class FakeStoreManagerService : IStoreManagerService
    {
        public ServiceResult<StoreResponse> GetStoreResult { get; set; } = ServiceResult<StoreResponse>.Success(new StoreResponse());
        public ServiceResult<StoreResponse> UpdateStoreResult { get; set; } = ServiceResult<StoreResponse>.Success(new StoreResponse());
        public ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>> ReplaceOpeningHoursResult { get; set; } =
            ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>.Success(Array.Empty<OpeningHoursEntryResponse>());
        public ServiceResult<IReadOnlyCollection<SlotRuleResponse>> ReplaceSlotRulesResult { get; set; } =
            ServiceResult<IReadOnlyCollection<SlotRuleResponse>>.Success(Array.Empty<SlotRuleResponse>());
        public ServiceResult<IReadOnlyCollection<WindowResponse>> GenerateSlotsResult { get; set; } = ServiceResult<IReadOnlyCollection<WindowResponse>>.Success(Array.Empty<WindowResponse>());
        public ServiceResult<WindowResponse> UpdateSlotStatusResult { get; set; } = ServiceResult<WindowResponse>.Success(new WindowResponse());
        public ServiceResult<SlotAllocationResponse> GetSlotAllocationsResult { get; set; } = ServiceResult<SlotAllocationResponse>.Success(new SlotAllocationResponse());
        public ServiceResult<SlotAllocationResponse> UpdateSlotAllocationStatusResult { get; set; } = ServiceResult<SlotAllocationResponse>.Success(new SlotAllocationResponse());
        public ServiceResult<StoreClosureResponse> CreateClosureResult { get; set; } = ServiceResult<StoreClosureResponse>.Success(new StoreClosureResponse());
        public ServiceResult<bool> DeleteClosureResult { get; set; } = ServiceResult<bool>.Success(true);
        public DateTimeOffset? GetWindowsFrom { get; private set; }
        public DateTimeOffset? GetWindowsTo { get; private set; }
        public string? GetSlotAllocationsStoreId { get; private set; }
        public string? GetSlotAllocationsSlotId { get; private set; }
        public DateTimeOffset? GetSlotAllocationsStartAt { get; private set; }
        public string? UpdateSlotAllocationStatusStoreId { get; private set; }
        public string? UpdateSlotAllocationStatusSlotId { get; private set; }
        public string? UpdateSlotAllocationStatusAllocationId { get; private set; }
        public SlotAllocationStatusUpdateRequest? UpdateSlotAllocationStatusRequest { get; private set; }
        public string? CancelBookingStoreId { get; private set; }
        public string? CancelBookingId { get; private set; }
        public BookingCancellationRequest? CancelBookingRequest { get; private set; }

        public Task<IReadOnlyCollection<StoreResponse>> GetStores() => Task.FromResult<IReadOnlyCollection<StoreResponse>>(Array.Empty<StoreResponse>());
        public Task<ServiceResult<StoreResponse>> GetStore(string id) => Task.FromResult(GetStoreResult);
        public Task<ServiceResult<StoreResponse>> UpdateStore(string id, StoreRequest request) => Task.FromResult(UpdateStoreResult);
        public Task<ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>> GetOpeningHours(string storeId) => throw new NotImplementedException();
        public Task<ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>> ReplaceOpeningHours(string storeId, OpeningHoursReplaceRequest request) =>
            Task.FromResult(ReplaceOpeningHoursResult);
        public Task<ServiceResult<IReadOnlyCollection<SlotRuleResponse>>> GetSlotRules(string storeId) => throw new NotImplementedException();
        public Task<ServiceResult<IReadOnlyCollection<SlotRuleResponse>>> ReplaceSlotRules(string storeId, SlotRuleReplaceRequest request) =>
            Task.FromResult(ReplaceSlotRulesResult);
        public Task<ServiceResult<IReadOnlyCollection<WindowResponse>>> GetWindows(string storeId, DateTimeOffset from, DateTimeOffset to)
        {
            GetWindowsFrom = from;
            GetWindowsTo = to;
            return Task.FromResult(ServiceResult<IReadOnlyCollection<WindowResponse>>.Success(Array.Empty<WindowResponse>()));
        }
        public Task<ServiceResult<WindowResponse>> GetSlot(string windowId) => throw new NotImplementedException();
        public Task<ServiceResult<IReadOnlyCollection<WindowResponse>>> GenerateSlots(string storeId) => Task.FromResult(GenerateSlotsResult);
        public Task<ServiceResult<WindowResponse>> UpdateSlotStatus(string storeId, string slotId, SlotStatusUpdateRequest request) => Task.FromResult(UpdateSlotStatusResult);
        public Task<ServiceResult<SlotAllocationResponse>> GetSlotAllocations(string storeId, string slotId, DateTimeOffset startAt)
        {
            GetSlotAllocationsStoreId = storeId;
            GetSlotAllocationsSlotId = slotId;
            GetSlotAllocationsStartAt = startAt;
            return Task.FromResult(GetSlotAllocationsResult);
        }
        public Task<ServiceResult<SlotAllocationResponse>> UpdateSlotAllocationStatus(string storeId, string slotId, string allocationId, SlotAllocationStatusUpdateRequest request)
        {
            UpdateSlotAllocationStatusStoreId = storeId;
            UpdateSlotAllocationStatusSlotId = slotId;
            UpdateSlotAllocationStatusAllocationId = allocationId;
            UpdateSlotAllocationStatusRequest = request;
            return Task.FromResult(UpdateSlotAllocationStatusResult);
        }
        public Task<ServiceResult<IReadOnlyCollection<AvailabilityWindowResponse>>> GetAvailability(string storeId, DateOnly date) => throw new NotImplementedException();
        public Task<ServiceResult<IReadOnlyCollection<AvailabilityWindowResponse>>> GetAvailability(string storeId, DateOnly from, DateOnly to) => throw new NotImplementedException();
        public Task<ServiceResult<IReadOnlyCollection<StoreClosureResponse>>> GetClosures(string storeId, DateOnly from, DateOnly to) => throw new NotImplementedException();
        public Task<ServiceResult<StoreClosureResponse>> CreateClosure(string storeId, StoreClosureRequest request) => Task.FromResult(CreateClosureResult);
        public Task<ServiceResult<bool>> DeleteClosure(string storeId, string closureId) => Task.FromResult(DeleteClosureResult);
        public Task<ServiceResult<IReadOnlyCollection<ManageBookingResponse>>> GetBookings(string storeId, DateOnly date, string? status, string? query) => throw new NotImplementedException();
        public Task<ServiceResult<ManageBookingResponse>> CancelBooking(string storeId, string bookingId, BookingCancellationRequest request)
        {
            CancelBookingStoreId = storeId;
            CancelBookingId = bookingId;
            CancelBookingRequest = request;
            return Task.FromResult(ServiceResult<ManageBookingResponse>.Success(new ManageBookingResponse { Id = bookingId, StoreId = storeId, Status = "CANCELLED" }));
        }
    }

}
