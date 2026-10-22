using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager;

internal sealed class StoreManagerScheduleRepository
{
    private readonly StoreAppointmentDbContext _dbContext;

    public StoreManagerScheduleRepository(StoreAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>> GetOpeningHours(string storeId)
    {
        var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerOpeningHoursRow>(StoredProcedures.StoreManagerGetOpeningHours, new SqlParameter("@StoreId", storeId.ToString()));
        return ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>.Success(rows
            .Select(x => new OpeningHoursEntryResponse
            {
                DayOfWeek = ConvertDbDayOfWeekToApi(x.DayOfWeek),
                OpensAt = x.IsClosed ? "closed" : x.OpensAt.ToString(@"hh\:mm"),
                ClosesAt = x.IsClosed ? "closed" : x.ClosesAt.ToString(@"hh\:mm")
            })
            .ToArray());
    }

    public async Task<ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>> ReplaceOpeningHours(string storeId, OpeningHoursReplaceRequest request)
    {
        if (request.OpeningHours is null)
        {
            return ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>.ValidationError("openingHours is required.");
        }

        var openingHoursRows = request.OpeningHours.ToArray();
        var validationError = ValidateOpeningHours(openingHoursRows);
        if (validationError is not null)
        {
            return ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>.ValidationError(validationError);
        }

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                var conflicts = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerBookingConflictRow>(
                    StoredProcedures.StoreManagerValidateOpeningHoursBookingImpact,
                    new SqlParameter("@StoreId", storeId),
                    BuildOpeningHoursTableParameter(openingHoursRows));
                var conflictResult = StoreManagerBookingConflict.Create<IReadOnlyCollection<OpeningHoursEntryResponse>>(
                    storeId,
                    conflicts);
                if (conflictResult is not null)
                {
                    return conflictResult;
                }

                await ReplaceOpeningHoursAndGenerateSlots(storeId, openingHoursRows);
                await transaction.CommitAsync();
            }
            else
            {
                await ReplaceOpeningHoursAndGenerateSlots(storeId, openingHoursRows);
            }
        }
        catch (SqlException ex) when (ex.Number is 50024 or 50025)
        {
            return ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>.ValidationError(ex.Message);
        }
        catch (FormatException)
        {
            return ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>.ValidationError("Invalid opening-hours time value. Expected 'HH:mm' or 'closed'.");
        }

        return await GetOpeningHours(storeId);
    }

    public async Task<ServiceResult<IReadOnlyCollection<SlotRuleResponse>>> GetSlotRules(
     string storeId)
    {
        var openingHours =
            await _dbContext.ExecuteStoredProcedureAsync<StoreManagerOpeningHoursRow>(
                StoredProcedures.StoreManagerGetOpeningHours,
                new SqlParameter("@StoreId", storeId));

        var slotRules =
            await _dbContext.ExecuteStoredProcedureAsync<StoreManagerSlotRuleRow>(
                StoredProcedures.StoreManagerGetSlotRules,
                new SqlParameter("@StoreId", storeId));

        var openingHourResponses = openingHours
            .Select(MapOpeningHours)
            .ToArray();

        var slotRuleResponses = slotRules
            .Select(MapSlotRule)
            .ToArray();

        return ServiceResult<IReadOnlyCollection<SlotRuleResponse>>.Success(
            SlotRuleResponseHelper.Build(
                storeId,
                openingHourResponses,
                slotRuleResponses));
    }

    public async Task<ServiceResult<IReadOnlyCollection<SlotRuleResponse>>> ReplaceSlotRules(string storeId, SlotRuleReplaceRequest request)
    {
        if (request.Rules is null)
        {
            return ServiceResult<IReadOnlyCollection<SlotRuleResponse>>.ValidationError("rules is required.");
        }

        var rules = request.Rules.ToArray();
        var validationError = ValidateSlotRules(rules);
        if (validationError is not null)
        {
            return ServiceResult<IReadOnlyCollection<SlotRuleResponse>>.ValidationError(validationError);
        }

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                var conflicts = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerBookingConflictRow>(
                    StoredProcedures.StoreManagerValidateSlotRulesBookingImpact,
                    new SqlParameter("@StoreId", storeId),
                    BuildSlotRulesTableParameter(rules));
                var conflictResult = StoreManagerBookingConflict.Create<IReadOnlyCollection<SlotRuleResponse>>(
                    storeId,
                    conflicts);
                if (conflictResult is not null)
                {
                    return conflictResult;
                }

                await ReplaceSlotRulesAndGenerateSlots(storeId, rules);
                await transaction.CommitAsync();
            }
            else
            {
                await ReplaceSlotRulesAndGenerateSlots(storeId, rules);
            }
        }
        catch (SqlException ex) when (ex.Number is 50011 or 50012 or 50021 or 50022 or 50023)
        {
            return ServiceResult<IReadOnlyCollection<SlotRuleResponse>>.ValidationError(ex.Message);
        }
        catch (FormatException)
        {
            return ServiceResult<IReadOnlyCollection<SlotRuleResponse>>.ValidationError("Invalid slot-rule time value. Expected 'HH:mm'.");
        }

        return await GetSlotRules(storeId);
    }

    public async Task<ServiceResult<IReadOnlyCollection<WindowResponse>>> GetWindows(string storeId, DateTimeOffset from, DateTimeOffset to)
    {
        var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerSlotRow>(StoredProcedures.StoreManagerGetSlots,
            new SqlParameter("@StoreId", storeId.ToString()),
            new SqlParameter("@FromUtc", from),
            new SqlParameter("@ToUtc", to));

        return ServiceResult<IReadOnlyCollection<WindowResponse>>.Success(rows.Select(MapWindow).ToArray());
    }

    public async Task<ServiceResult<WindowResponse>> GetSlot(string storeSlotId)
    {
        if (!int.TryParse(storeSlotId, out var parsedStoreSlotId))
        {
            return ServiceResult<WindowResponse>.NotFound($"Store slot '{storeSlotId}' was not found.");
        }

        var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerSlotRow>(StoredProcedures.StoreManagerGetSlotById,
            new SqlParameter("@StoreSlotId", parsedStoreSlotId));
        var row = rows.FirstOrDefault();
        return row is null ? ServiceResult<WindowResponse>.NotFound($"Store slot '{storeSlotId}' was not found.") : ServiceResult<WindowResponse>.Success(MapWindow(row));
    }

    public async Task<ServiceResult<IReadOnlyCollection<WindowResponse>>> GenerateSlots(string storeId)
    {
        await _dbContext.ExecuteStoredProcedureAsync(StoredProcedures.StoreManagerGenerateSlots,
            new SqlParameter("@StoreId", storeId));

        return ServiceResult<IReadOnlyCollection<WindowResponse>>.Success(Array.Empty<WindowResponse>());
    }

    public async Task<ServiceResult<WindowResponse>> UpdateSlotStatus(string storeId, string slotId, SlotStatusUpdateRequest request)
    {
        if (request.Status is null)
        {
            return ServiceResult<WindowResponse>.ValidationError("status is required.");
        }

        if (request.StartAt is null)
        {
            return ServiceResult<WindowResponse>.ValidationError("startAt is required.");
        }

        if (request.EndAt is null)
        {
            return ServiceResult<WindowResponse>.ValidationError("endAt is required.");
        }

        var startAt = request.StartAt.Value;
        var endAt = request.EndAt.Value;
        if (endAt <= startAt)
        {
            return ServiceResult<WindowResponse>.ValidationError("endAt must be greater than startAt.");
        }

        if (startAt.Date != endAt.Date)
        {
            return ServiceResult<WindowResponse>.ValidationError("startAt and endAt must be on the same date.");
        }

        try
        {
            if (!TryParseStoreSlotId(slotId, out var parsedStoreSlotId))
            {
                return ServiceResult<WindowResponse>.NotFound($"Store slot '{slotId}' was not found for store '{storeId}'.");
            }

            var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerSlotRow>(StoredProcedures.StoreManagerUpdateSlotStatus,
                new SqlParameter("@StoreId", storeId),
                new SqlParameter("@StoreSlotId", parsedStoreSlotId),
                new SqlParameter("@Status", request.Status.Value.ToString().ToUpperInvariant()),
                new SqlParameter("@SlotDate", startAt.Date),
                new SqlParameter("@SlotStart", startAt.TimeOfDay),
                new SqlParameter("@SlotEnd", endAt.TimeOfDay),
                new SqlParameter("@Reason", (object?)request.Reason ?? DBNull.Value));

            var row = rows.FirstOrDefault();
            return row is null
                ? ServiceResult<WindowResponse>.NotFound($"Store slot '{slotId}' was not found for store '{storeId}'.")
                : ServiceResult<WindowResponse>.Success(MapWindow(row));
        }
        catch (SqlException ex) when (ex.Number == 50031)
        {
            return ServiceResult<WindowResponse>.NotFound($"Store slot '{slotId}' was not found for store '{storeId}'.");
        }
        catch (SqlException ex) when (ex.Number == 50032)
        {
            return ServiceResult<WindowResponse>.ValidationError(ex.Message);
        }
    }

    public async Task<ServiceResult<SlotAllocationResponse>> GetSlotAllocations(string storeId, string slotId, DateTimeOffset startAt)
    {
        if (!TryParseStoreSlotId(slotId, out var parsedStoreSlotId))
        {
            return ServiceResult<SlotAllocationResponse>.NotFound($"Store slot '{slotId}' was not found for store '{storeId}'.");
        }

        try
        {
            var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerSlotAllocationRow>(
                StoredProcedures.StoreManagerGetSlotAllocations,
                new SqlParameter("@StoreId", storeId),
                new SqlParameter("@StoreSlotId", parsedStoreSlotId),
                new SqlParameter("@SlotDate", startAt.Date));

            return MapSlotAllocationResult(rows, storeId, slotId);
        }
        catch (SqlException ex) when (ex.Number == 50041)
        {
            return ServiceResult<SlotAllocationResponse>.NotFound($"Store slot '{slotId}' was not found for store '{storeId}'.");
        }
    }

    public async Task<ServiceResult<SlotAllocationResponse>> UpdateSlotAllocationStatus(string storeId, string slotId, string allocationId, SlotAllocationStatusUpdateRequest request)
    {
        if (request is null)
        {
            return ServiceResult<SlotAllocationResponse>.ValidationError("request is required.");
        }

        if (string.IsNullOrWhiteSpace(allocationId))
        {
            return ServiceResult<SlotAllocationResponse>.ValidationError("allocationId is required.");
        }

        if (request.Status is null)
        {
            return ServiceResult<SlotAllocationResponse>.ValidationError("status is required.");
        }

        if (request.Status.Value is not SlotStatus.Available and not SlotStatus.Blocked)
        {
            return ServiceResult<SlotAllocationResponse>.ValidationError("Only AVAILABLE and BLOCKED allocation updates are supported.");
        }

        if (!TryParseStoreSlotId(slotId, out var parsedStoreSlotId))
        {
            return ServiceResult<SlotAllocationResponse>.NotFound($"Store slot '{slotId}' was not found for store '{storeId}'.");
        }

        try
        {
            var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerSlotAllocationRow>(
                StoredProcedures.StoreManagerUpdateSlotAllocationStatus,
                new SqlParameter("@StoreId", storeId),
                new SqlParameter("@StoreSlotId", parsedStoreSlotId),
                new SqlParameter("@AllocationId", allocationId),
                new SqlParameter("@Status", request.Status.Value.ToString().ToUpperInvariant()),
                new SqlParameter("@Reason", (object?)request.Reason ?? DBNull.Value));

            return MapSlotAllocationResult(rows, storeId, slotId);
        }
        catch (SqlException ex) when (ex.Number == 50041)
        {
            return ServiceResult<SlotAllocationResponse>.NotFound($"Store slot '{slotId}' was not found for store '{storeId}'.");
        }
        catch (SqlException ex) when (ex.Number is 50042 or 50043 or 50044 or 50045)
        {
            return ServiceResult<SlotAllocationResponse>.ValidationError(ex.Message);
        }
    }

    public Task<ServiceResult<IReadOnlyCollection<AvailabilityWindowResponse>>> GetAvailability(string storeId, DateOnly date) => GetAvailability(storeId, date, date);

    public async Task<ServiceResult<IReadOnlyCollection<AvailabilityWindowResponse>>> GetAvailability(string storeId, DateOnly from, DateOnly to)
    {
        var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerSlotRow>(StoredProcedures.StoreManagerGetAvailability,
            new SqlParameter("@StoreId", storeId),
            new SqlParameter("@FromDate", from),
            new SqlParameter("@ToDate", to));

        return ServiceResult<IReadOnlyCollection<AvailabilityWindowResponse>>.Success(rows.Select(x => new AvailabilityWindowResponse
        {
            Id = x.StoreSlotId.ToString(CultureInfo.InvariantCulture),
            StoreId = ParseString(x.StoreId),
            StartAt = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(x.StartsAtUtc),
            EndAt = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(x.EndsAtUtc),
            Capacity = x.Capacity,
            RemainingCapacity = x.RemainingCapacity
        }).ToArray());
    }

    private async Task ExecuteSlotGeneration(string storeId)
    {
        await _dbContext.ExecuteStoredProcedureAsync(StoredProcedures.StoreManagerGenerateSlots,
            new SqlParameter("@StoreId", storeId));
    }

    private async Task ReplaceOpeningHoursAndGenerateSlots(
        string storeId,
        IReadOnlyCollection<OpeningHoursEntryRequest> openingHours)
    {
        await _dbContext.ExecuteStoredProcedureAsync(
            StoredProcedures.StoreManagerReplaceOpeningHours,
            new SqlParameter("@StoreId", storeId),
            BuildOpeningHoursTableParameter(openingHours));
        await ExecuteSlotGeneration(storeId);
    }

    private async Task ReplaceSlotRulesAndGenerateSlots(
        string storeId,
        IReadOnlyCollection<SlotRuleRequest> rules)
    {
        await _dbContext.ExecuteStoredProcedureAsync(
            StoredProcedures.StoreManagerReplaceSlotRules,
            new SqlParameter("@StoreId", storeId),
            BuildSlotRulesTableParameter(rules));
        await ExecuteSlotGeneration(storeId);
    }

    private static SqlParameter BuildOpeningHoursTableParameter(IEnumerable<OpeningHoursEntryRequest> openingHours)
    {
        var openingHoursTable = new DataTable();
        openingHoursTable.Columns.Add("DayOfWeek", typeof(byte));
        openingHoursTable.Columns.Add("OpensAt", typeof(TimeSpan));
        openingHoursTable.Columns.Add("ClosesAt", typeof(TimeSpan));

        foreach (var item in openingHours)
        {
            var isClosed = string.Equals(item.OpensAt, "closed", StringComparison.OrdinalIgnoreCase)
                           && string.Equals(item.ClosesAt, "closed", StringComparison.OrdinalIgnoreCase);
            if (isClosed)
            {
                openingHoursTable.Rows.Add(ConvertApiDayOfWeekToDb(item.DayOfWeek), TimeSpan.Zero, TimeSpan.Zero);
                continue;
            }

            openingHoursTable.Rows.Add(ConvertApiDayOfWeekToDb(item.DayOfWeek), ParseOpeningHoursTime(item.OpensAt), ParseOpeningHoursTime(item.ClosesAt));
        }

        return new SqlParameter("@OpeningHours", SqlDbType.Structured)
        {
            TypeName = "dbo.StoreManagerOpeningHoursType",
            Value = openingHoursTable
        };
    }

    private static string? ValidateOpeningHours(IEnumerable<OpeningHoursEntryRequest> openingHours)
    {
        return openingHours.Any(item => item.DayOfWeek is < 0 or > 6)
            ? "dayOfWeek must be between 0 and 6."
            : null;
    }

    private static TimeSpan ParseOpeningHoursTime(string value)
    {
        return TimeSpan.ParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture);
    }
    private static OpeningHoursEntryResponse MapOpeningHours(StoreManagerOpeningHoursRow row) => new()
    {
        DayOfWeek = ConvertDbDayOfWeekToApi(row.DayOfWeek),
        OpensAt = row.IsClosed
        ? "closed"
        : row.OpensAt.ToString(@"hh\:mm"),
        ClosesAt = row.IsClosed
        ? "closed"
        : row.ClosesAt.ToString(@"hh\:mm")
    };

    private static SlotRuleResponse MapSlotRule(StoreManagerSlotRuleRow x) => new()
    {
        Id = ParseString(x.SlotRuleId),
        DayOfWeek = ConvertDbDayOfWeekToApi(x.DayOfWeek),
        StartTime = x.StartTime.ToString(@"hh\:mm"),
        EndTime = x.EndTime.ToString(@"hh\:mm"),
        SlotDurationMinutes = x.SlotDurationMinutes,
        SlotCapacity = x.SlotCapacity
    };

    private static SqlParameter BuildSlotRulesTableParameter(IEnumerable<SlotRuleRequest> rules)
    {
        var slotRulesTable = new DataTable();
        slotRulesTable.Columns.Add("DayOfWeek", typeof(byte));
        slotRulesTable.Columns.Add("StartTime", typeof(TimeSpan));
        slotRulesTable.Columns.Add("EndTime", typeof(TimeSpan));
        slotRulesTable.Columns.Add("SlotDurationMinutes", typeof(int));
        slotRulesTable.Columns.Add("SlotCapacity", typeof(int));

        foreach (var item in rules)
        {
            slotRulesTable.Rows.Add(ConvertApiDayOfWeekToDb(item.DayOfWeek), TimeSpan.ParseExact(item.StartTime, @"hh\:mm", CultureInfo.InvariantCulture), TimeSpan.ParseExact(item.EndTime, @"hh\:mm", CultureInfo.InvariantCulture), item.SlotDurationMinutes, item.SlotCapacity);
        }

        return new SqlParameter("@Rules", SqlDbType.Structured)
        {
            TypeName = "dbo.StoreManagerSlotRuleType",
            Value = slotRulesTable
        };
    }

    private static string? ValidateSlotRules(IEnumerable<SlotRuleRequest> rules)
    {
        if (rules.Any(item => item.DayOfWeek is < 0 or > 6))
        {
            return "dayOfWeek must be between 0 and 6.";
        }

        if (rules.Any(item => item.SlotDurationMinutes is not (30 or 60)))
        {
            return "slotDurationMinutes must be either 30 or 60.";
        }

        if (rules.Any(item => item.SlotCapacity <= 0))
        {
            return "slotCapacity must be greater than zero.";
        }

        return null;
    }

    private static WindowResponse MapWindow(StoreManagerSlotRow x) => new()
    {
        Id = x.StoreSlotId.ToString(CultureInfo.InvariantCulture),
        StoreId = ParseString(x.StoreId),
        StartAt = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(x.StartsAtUtc),
        EndAt = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(x.EndsAtUtc),
        Capacity = x.Capacity,
        RemainingCapacity = x.RemainingCapacity,
        Status = ParseSlotStatus(x.Status),
        Reason = x.Reason
    };

    private static ServiceResult<SlotAllocationResponse> MapSlotAllocationResult(
        IReadOnlyCollection<StoreManagerSlotAllocationRow> rows,
        string storeId,
        string slotId)
    {
        var first = rows.FirstOrDefault();
        if (first is null)
        {
            return ServiceResult<SlotAllocationResponse>.NotFound($"Store slot '{slotId}' was not found for store '{storeId}'.");
        }

        var responseSlotId = slotId.Trim();
        var response = new SlotAllocationResponse
        {
            Id = responseSlotId,
            WindowId = responseSlotId,
            SlotId = responseSlotId,
            StoreId = ParseString(first.StoreId),
            StartAt = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(first.StartsAtUtc),
            EndAt = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(first.EndsAtUtc),
            Capacity = first.Capacity,
            BookedCount = first.BookedCount,
            RemainingCapacity = first.RemainingCapacity,
            Allocations = rows
                .OrderBy(row => row.AllocationIndex)
                .Select(row => new SlotAllocationItemResponse
                {
                    Id = row.AllocationId ?? string.Empty,
                    WindowId = responseSlotId,
                    SlotId = responseSlotId,
                    StoreId = ParseString(row.StoreId),
                    AllocationIndex = row.AllocationIndex,
                    Status = ParseSlotStatus(row.Status),
                    Reason = row.Reason,
                    BookingId = string.IsNullOrWhiteSpace(row.BookingId) ? null : row.BookingId,
                    CustomerRef = string.IsNullOrWhiteSpace(row.CustomerRef) ? null : row.CustomerRef
                })
                .ToArray()
        };

        return ServiceResult<SlotAllocationResponse>.Success(response);
    }

    private static bool TryParseStoreSlotId(string slotId, out int parsedStoreSlotId)
    {
        const string SlotPrefix = "slot-";
        if (string.IsNullOrWhiteSpace(slotId))
        {
            parsedStoreSlotId = default;
            return false;
        }

        var candidate = slotId.Trim();
        if (candidate.StartsWith(SlotPrefix, StringComparison.OrdinalIgnoreCase))
        {
            candidate = candidate[SlotPrefix.Length..];
        }

        return int.TryParse(candidate, NumberStyles.None, CultureInfo.InvariantCulture, out parsedStoreSlotId);
    }

    private static SlotStatus ParseSlotStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return SlotStatus.Available;
        }

        return Enum.TryParse<SlotStatus>(status, true, out var parsedStatus)
            ? parsedStatus
            : throw new InvalidOperationException($"Unsupported slot status '{status}'.");
    }
    private static byte ConvertApiDayOfWeekToDb(int apiDayOfWeek)
    {
        if (apiDayOfWeek is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(apiDayOfWeek), "DayOfWeek must be between 0 and 6.");
        }

        return (byte)apiDayOfWeek;
    }

    private static int ConvertDbDayOfWeekToApi(byte dbDayOfWeek)
    {
        if (dbDayOfWeek > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(dbDayOfWeek), "Stored DayOfWeek must be between 0 and 6.");
        }

        return dbDayOfWeek;
    }

    private static string ParseString(string? raw) => raw?.Trim() ?? string.Empty;
}
