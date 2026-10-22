using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager;
using StoreAppointmentConstants = VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants.Constants;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests;

public class StoreManagerRepositoryStoreUpdateTests
{
    private readonly IServiceProvider _serviceProvider;

    public StoreManagerRepositoryStoreUpdateTests()
    {
        _serviceProvider = new ServiceCollection().AddLogging()
                                                  .AddDbContext<StoreAppointmentDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
                                                  .AddScoped<StoreManagerRepository>()
                                                  .BuildServiceProvider();
    }

    [Fact]
    public async Task UpdateStore_ShouldPersistSelectedServices_WhenServicesProvided()
    {
        var dbContext = _serviceProvider.GetRequiredService<StoreAppointmentDbContext>();
        dbContext.Stores.Add(BuildStore("store-1", "Welcome drinks and snacks", "Aircraft seats"));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore("store-1", new StoreRequest
        {
            Name = "Updated store",
            AddressLine1 = "1 Updated Street",
            CityTown = "Updated City",
            PostalCode = "AB1 2CD",
            Telephone = "01234567890",
            Email = "updated@example.com",
            Services = new[] { StoreAppointmentConstants.KidsArea.ToUpperInvariant(), "Secret cinema" },
            IsClosed = true
        });

        var updatedStore = await dbContext.Stores.SingleAsync(
            store => store.StoreId == "store-1",
            TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(updatedStore.MarkedAsClosed);
        Assert.True(result.Value!.IsClosed);
        Assert.Equal("[\"Kids area\",\"Secret cinema\"]", updatedStore.StoreServicesJson);
        Assert.Contains("Kids area", result.Value.Services);
        Assert.Contains("Secret cinema", result.Value.Services);
        Assert.DoesNotContain("Welcome drinks and snacks", result.Value.Services);
        Assert.DoesNotContain("Aircraft seats", result.Value.Services);
    }

    [Fact]
    public async Task UpdateStore_ShouldDeactivateSlots_WhenStoreIsMarkedClosed()
    {
        var dbContext = _serviceProvider.GetRequiredService<StoreAppointmentDbContext>();
        dbContext.Stores.Add(BuildStore("store-closed"));
        dbContext.StoreSlots.Add(new StoreSlotEntity
        {
            StoreId = "store-closed",
            DayCode = "Mon",
            SlotStartTime = new TimeOnly(10, 0),
            SlotEndTime = new TimeOnly(11, 0),
            Capacity = 1,
            IsActive = true
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore("store-closed", new StoreRequest
        {
            Name = "Closed store",
            AddressLine1 = "1 Closed Street",
            CityTown = "Closed City",
            PostalCode = "CL1 1AA",
            IsClosed = true
        });

        var slot = await dbContext.StoreSlots.SingleAsync(
            item => item.StoreId == "store-closed",
            TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.False(slot.IsActive);
    }

    [Fact]
    public async Task UpdateStore_ShouldDeactivateSlots_WhenStoreIdHasWhitespace()
    {
        var dbContext = _serviceProvider.GetRequiredService<StoreAppointmentDbContext>();
        dbContext.Stores.Add(BuildStore("store-trimmed"));
        dbContext.StoreSlots.Add(new StoreSlotEntity
        {
            StoreId = "store-trimmed",
            DayCode = "Mon",
            SlotStartTime = new TimeOnly(10, 0),
            SlotEndTime = new TimeOnly(11, 0),
            Capacity = 1,
            IsActive = true
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore(" store-trimmed ", new StoreRequest
        {
            Name = "Closed store",
            AddressLine1 = "1 Closed Street",
            CityTown = "Closed City",
            PostalCode = "CL1 1AA",
            IsClosed = true
        });

        var slot = await dbContext.StoreSlots.SingleAsync(
            item => item.StoreId == "store-trimmed",
            TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal("store-trimmed", result.Value!.Id);
        Assert.False(slot.IsActive);
    }

    [Fact]
    public async Task UpdateStore_ShouldPreserveExistingServices_WhenServicesAreMissing()
    {
        var dbContext = _serviceProvider.GetRequiredService<StoreAppointmentDbContext>();
        dbContext.Stores.Add(BuildStore("store-2", "Kids area", "Aircraft seats"));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore("store-2", new StoreRequest
        {
            Name = "Updated store",
            AddressLine1 = "2 Updated Street",
            CityTown = "Updated City",
            PostalCode = "CD3 4EF"
        });

        var updatedStore = await dbContext.Stores.SingleAsync(
            store => store.StoreId == "store-2",
            TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("[\"Kids area\",\"Aircraft seats\"]", updatedStore.StoreServicesJson);
        Assert.Equal("Original County", updatedStore.County);
        Assert.Equal("01999999999", updatedStore.Phone);
        Assert.Equal("original@example.com", updatedStore.EmailAddress);
        Assert.Contains("Kids area", result.Value!.Services);
        Assert.Contains("Aircraft seats", result.Value.Services);
        Assert.DoesNotContain("Welcome drinks and snacks", result.Value.Services);
        Assert.DoesNotContain("Secret cinema", result.Value.Services);
    }

    [Fact]
    public async Task UpdateStore_ShouldPersistDynamicServicesInJson()
    {
        var dbContext = _serviceProvider.GetRequiredService<StoreAppointmentDbContext>();
        dbContext.Stores.Add(BuildStore("store-3"));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore("store-3", new StoreRequest
        {
            Name = "Updated store",
            AddressLine1 = "3 Updated Street",
            CityTown = "Updated City",
            PostalCode = "EF5 6GH",
            Services = ["Travel lounge", "Kids Area"]
        });

        var updatedStore = await dbContext.Stores.SingleAsync(
            store => store.StoreId == "store-3",
            TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal("[\"Travel lounge\",\"Kids area\"]", updatedStore.StoreServicesJson);
        Assert.Contains("Travel lounge", result.Value!.Services);
        Assert.Contains("Kids area", result.Value.Services);
    }

    [Fact]
    public async Task UpdateStore_ShouldClearServices_WhenEmptyArrayIsProvided()
    {
        var dbContext = _serviceProvider.GetRequiredService<StoreAppointmentDbContext>();
        dbContext.Stores.Add(BuildStore("store-4", "Kids area", "Secret cinema"));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore("store-4", new StoreRequest
        {
            Name = "Updated store",
            AddressLine1 = "4 Updated Street",
            CityTown = "Updated City",
            PostalCode = "GH7 8IJ",
            Services = []
        });

        var updatedStore = await dbContext.Stores.SingleAsync(
            store => store.StoreId == "store-4",
            TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal("[]", updatedStore.StoreServicesJson);
        Assert.Empty(result.Value!.Services);
    }

    [Fact]
    public async Task GetStore_ShouldReturnEmptyServices_WhenServicesJsonIsInvalid()
    {
        var dbContext = _serviceProvider.GetRequiredService<StoreAppointmentDbContext>();
        var store = BuildStore("store-5", "Kids area");
        store.StoreServicesJson = "not-json";
        dbContext.Stores.Add(store);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.GetStore("store-5");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Services);
    }

    [Fact]
    public async Task UpdateStore_ShouldNormalizeAndDeduplicateServices()
    {
        var dbContext = _serviceProvider.GetRequiredService<StoreAppointmentDbContext>();
        dbContext.Stores.Add(BuildStore("store-6"));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore("store-6", new StoreRequest
        {
            Name = "Updated store",
            AddressLine1 = "6 Updated Street",
            CityTown = "Updated City",
            PostalCode = "KL1 2MN",
            Services = [" kids area ", "KIDS AREA", "Travel lounge", "travel lounge"]
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(["Kids area", "Travel lounge"], result.Value!.Services);
    }

    [Fact]
    public async Task UpdateStore_ShouldReturnValidationError_WhenTelephoneHasSpaces()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore("store-1", new StoreRequest
        {
            Name = "Updated store",
            AddressLine1 = "1 Updated Street",
            CityTown = "Updated City",
            PostalCode = "AB1 2CD",
            Telephone = "01202 111111"
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("Telephone must be a valid UK number with no spaces.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateStore_ShouldReturnValidationError_WhenTelephoneIsNotUkFormat()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore("store-1", new StoreRequest
        {
            Name = "Updated store",
            AddressLine1 = "1 Updated Street",
            CityTown = "Updated City",
            PostalCode = "AB1 2CD",
            Telephone = "1234567890"
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("Telephone must be a valid UK number with no spaces.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("person@example")]
    [InlineData("person@localhost")]
    [InlineData("person@123")]
    [InlineData("person@[192.168.1.1]")]
    [InlineData("\"person name\"@example.com")]
    [InlineData("person..name@example.com")]
    [InlineData(".person@example.com")]
    [InlineData("person.@example.com")]
    [InlineData("person@example..com")]
    [InlineData("person@example.com.")]
    [InlineData("person@-example.com")]
    [InlineData("person@example-.com")]
    [InlineData("person@exa_mple.com")]
    [InlineData("person@example.c")]
    [InlineData("person@example.123")]
    public async Task UpdateStore_ShouldReturnValidationError_WhenEmailIsInvalid(string email)
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore("store-1", new StoreRequest
        {
            Name = "Updated store",
            AddressLine1 = "1 Updated Street",
            CityTown = "Updated City",
            PostalCode = "AB1 2CD",
            Telephone = "01202111111",
            Email = email
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("Email must be a valid email address.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("updated@example.com")]
    [InlineData(" updated@example.com ")]
    [InlineData("person.name@example.co.uk")]
    [InlineData("person+tag@example.travel")]
    [InlineData("person_name@example.com")]
    [InlineData("person-name@example.com")]
    [InlineData("person.name+tag@sub.example.com")]
    [InlineData("p@example.io")]
    [InlineData("person123@example-domain.com")]
    public async Task UpdateStore_ShouldPersistContactDetails_WhenTelephoneAndEmailAreValid(string email)
    {
        var dbContext = _serviceProvider.GetRequiredService<StoreAppointmentDbContext>();
        var storeId = $"store-{Guid.NewGuid():N}";
        dbContext.Stores.Add(BuildStore(storeId));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateStore(storeId, new StoreRequest
        {
            Name = "Updated store",
            AddressLine1 = "7 Updated Street",
            CityTown = "Updated City",
            PostalCode = "MN3 4OP",
            Telephone = "01202111111",
            Email = email
        });

        var updatedStore = await dbContext.Stores.SingleAsync(
            store => store.StoreId == storeId,
            TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal("01202111111", updatedStore.Phone);
        Assert.Equal(email.Trim(), updatedStore.EmailAddress);
    }

    [Fact]
    public async Task UpdateSlotStatus_ShouldReturnValidationError_WhenStatusIsMissing()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateSlotStatus("store-1", "slot-1", new SlotStatusUpdateRequest());

        Assert.True(result.IsValidationError);
        Assert.Equal("status is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSlotStatus_ShouldReturnValidationError_WhenStartAtIsMissing()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateSlotStatus("store-1", "slot-1", new SlotStatusUpdateRequest
        {
            Status = SlotStatus.Blocked,
            EndAt = DateTimeOffset.UtcNow.AddHours(1)
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("startAt is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSlotStatus_ShouldReturnValidationError_WhenEndAtIsMissing()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateSlotStatus("store-1", "slot-1", new SlotStatusUpdateRequest
        {
            Status = SlotStatus.Booked,
            StartAt = DateTimeOffset.UtcNow
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("endAt is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSlotStatus_ShouldReturnValidationError_WhenEndAtIsNotAfterStartAt()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();
        var startsAt = new DateTimeOffset(2026, 6, 16, 9, 0, 0, TimeSpan.Zero);

        var result = await repository.UpdateSlotStatus("store-1", "slot-1", new SlotStatusUpdateRequest
        {
            Status = SlotStatus.Available,
            StartAt = startsAt,
            EndAt = startsAt
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("endAt must be greater than startAt.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSlotAllocationStatus_ShouldReturnValidationError_WhenAllocationIdIsMissing()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.UpdateSlotAllocationStatus(
            "store-1",
            "slot-1",
            string.Empty,
            new SlotAllocationStatusUpdateRequest { Status = SlotStatus.Blocked });

        Assert.True(result.IsValidationError);
        Assert.Equal("allocationId is required.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("123", true, 123)]
    [InlineData(" slot-123 ", true, 123)]
    [InlineData("SLOT-123", true, 123)]
    [InlineData("slot-abc", false, 0)]
    [InlineData(null, false, 0)]
    [InlineData("", false, 0)]
    [InlineData("   ", false, 0)]
    public void TryParseStoreSlotId_ShouldAcceptNumericAndPrefixedIds(string? slotId, bool expectedResult, int expectedParsedSlotId)
    {
        var scheduleRepositoryType = GetStoreManagerScheduleRepositoryType();
        var method = scheduleRepositoryType.GetMethod(
            "TryParseStoreSlotId",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var parameters = new object?[] { slotId, 0 };

        var result = (bool)method!.Invoke(null, parameters)!;

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedParsedSlotId, parameters[1]);
    }

    [Fact]
    public void MapSlotAllocationResult_ShouldPreserveRequestedSlotId()
    {
        var scheduleRepositoryType = GetStoreManagerScheduleRepositoryType();
        var rowType = typeof(StoreManagerRepository).Assembly.GetType(
            "VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models.StoreManagerSlotAllocationRow")!;
        var row = Activator.CreateInstance(rowType)!;
        rowType.GetProperty("StoreSlotId")!.SetValue(row, 123);
        rowType.GetProperty("StoreId")!.SetValue(row, "store-1");
        rowType.GetProperty("StartsAtUtc")!.SetValue(row, new DateTimeOffset(2026, 7, 15, 9, 0, 0, TimeSpan.Zero));
        rowType.GetProperty("EndsAtUtc")!.SetValue(row, new DateTimeOffset(2026, 7, 15, 9, 30, 0, TimeSpan.Zero));
        rowType.GetProperty("Capacity")!.SetValue(row, 1);
        rowType.GetProperty("RemainingCapacity")!.SetValue(row, 1);
        rowType.GetProperty("AllocationId")!.SetValue(row, "allocation-20260715-1");
        rowType.GetProperty("AllocationIndex")!.SetValue(row, 1);
        rowType.GetProperty("Status")!.SetValue(row, "AVAILABLE");
        var rows = Array.CreateInstance(rowType, 1);
        rows.SetValue(row, 0);
        var method = scheduleRepositoryType.GetMethod(
            "MapSlotAllocationResult",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (ServiceResult<SlotAllocationResponse>)method!.Invoke(null, new object[] { rows, "store-1", "slot-123" })!;

        Assert.True(result.IsSuccess);
        Assert.Equal("slot-123", result.Value!.Id);
        Assert.Equal("slot-123", result.Value.WindowId);
        Assert.Equal("slot-123", result.Value.SlotId);
        var allocation = Assert.Single(result.Value.Allocations);
        Assert.Equal("slot-123", allocation.WindowId);
        Assert.Equal("slot-123", allocation.SlotId);
    }

    private static Type GetStoreManagerScheduleRepositoryType() =>
        typeof(StoreManagerRepository).Assembly.GetType(
            "VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.StoreManagerScheduleRepository")!;

    [Fact]
    public async Task ReplaceOpeningHours_ShouldReturnValidationError_WhenOpeningHoursIsNull()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.ReplaceOpeningHours("store-1", new OpeningHoursReplaceRequest { OpeningHours = null! });

        Assert.True(result.IsValidationError);
        Assert.Equal("openingHours is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task ReplaceOpeningHours_ShouldReturnValidationError_WhenDayOfWeekIsOutOfRange()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.ReplaceOpeningHours("store-1", new OpeningHoursReplaceRequest
        {
            OpeningHours =
            [
                new OpeningHoursEntryRequest
                {
                    DayOfWeek = 7,
                    OpensAt = "09:00",
                    ClosesAt = "17:00"
                }
            ]
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("dayOfWeek must be between 0 and 6.", result.ErrorMessage);
    }

    [Fact]
    public async Task ReplaceSlotRules_ShouldReturnValidationError_WhenRulesIsNull()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.ReplaceSlotRules("store-1", new SlotRuleReplaceRequest { Rules = null! });

        Assert.True(result.IsValidationError);
        Assert.Equal("rules is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task ReplaceSlotRules_ShouldReturnValidationError_WhenDayOfWeekIsOutOfRange()
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.ReplaceSlotRules("store-1", new SlotRuleReplaceRequest
        {
            Rules =
            [
                new SlotRuleRequest
                {
                    DayOfWeek = 7,
                    StartTime = "09:00",
                    EndTime = "17:00",
                    SlotDurationMinutes = 60,
                    SlotCapacity = 1
                }
            ]
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("dayOfWeek must be between 0 and 6.", result.ErrorMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(45)]
    [InlineData(90)]
    public async Task ReplaceSlotRules_ShouldReturnValidationError_WhenDurationIsUnsupported(int slotDurationMinutes)
    {
        var repository = _serviceProvider.GetRequiredService<StoreManagerRepository>();

        var result = await repository.ReplaceSlotRules("store-1", new SlotRuleReplaceRequest
        {
            Rules =
            [
                new SlotRuleRequest
                {
                    DayOfWeek = 1,
                    StartTime = "09:00",
                    EndTime = "17:00",
                    SlotDurationMinutes = slotDurationMinutes,
                    SlotCapacity = 1
                }
            ]
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("slotDurationMinutes must be either 30 or 60.", result.ErrorMessage);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    public void ValidateSlotRules_ShouldAllowSupportedDurations(int slotDurationMinutes)
    {
        var method = GetStoreManagerScheduleRepositoryType().GetMethod(
            "ValidateSlotRules",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var rules = new[]
        {
            new SlotRuleRequest
            {
                DayOfWeek = 1,
                StartTime = "09:00",
                EndTime = "17:00",
                SlotDurationMinutes = slotDurationMinutes,
                SlotCapacity = 1
            }
        };

        var validationError = (string?)method!.Invoke(null, new object[] { rules });

        Assert.Null(validationError);
    }

    private static StoreEntity BuildStore(string storeId, params string[] services) => new()
        {
            StoreId = storeId,
            Name = "Original store",
            Label = "Original store",
            AddressLine1 = "1 Original Street",
            AddressLine2 = "Original address 2",
            City = "Original City",
            County = "Original County",
            CountryCode = "GB",
            PostalCode = "OR1 1AA",
            Phone = "01999999999",
            EmailAddress = "original@example.com",
            StoreServicesJson = System.Text.Json.JsonSerializer.Serialize(services)
        };
}
