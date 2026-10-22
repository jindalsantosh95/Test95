using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment.Models;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;

public class StoreAppointmentDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;

    public StoreAppointmentDbContext(DbContextOptions<StoreAppointmentDbContext> options)
        : this(options, TimeProvider.System)
    {
    }

    public StoreAppointmentDbContext(DbContextOptions<StoreAppointmentDbContext> options, TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider;
    }

    public DbSet<Appointment> StoreAppointments => Set<Appointment>();
    public DbSet<StoreEntity> Stores => Set<StoreEntity>();
    public DbSet<StoreSlotEntity> StoreSlots => Set<StoreSlotEntity>();
    public DbSet<StoreManagerClosureEntity> StoreManagerClosures => Set<StoreManagerClosureEntity>();
    public DbSet<StoreAppointmentReferenceResult> StoreAppointmentReferenceResults => Set<StoreAppointmentReferenceResult>();
    public DbSet<StoreSlotResult> StoreSlotResults => Set<StoreSlotResult>();
    public DbSet<StoreCalendarAvailabilityResultRow> StoreCalendarAvailabilityResults => Set<StoreCalendarAvailabilityResultRow>();

    public Task<List<TEntity>> ExecuteStoredProcedureAsync<TEntity>(string storedProcedureName, params SqlParameter[] parameters)
        where TEntity : class
    {
        if (!Database.IsRelational())
        {
            return ExecuteStoredProcedureInMemoryAsync<TEntity>(storedProcedureName, parameters);
        }

        var sql = BuildStoredProcedureSql(storedProcedureName, parameters);

        return Set<TEntity>()
            .FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> ExecuteStoredProcedureAsync(string storedProcedureName, params SqlParameter[] parameters)
    {
        if (!Database.IsRelational())
        {
            return await ExecuteStoredProcedureInMemoryAsync(storedProcedureName, parameters);
        }

        var sql = BuildStoredProcedureSql(storedProcedureName, parameters);
        return await Database.ExecuteSqlRawAsync(sql, parameters);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoreEntity>(entity =>
        {
            entity.HasKey(record => record.StoreId);
            entity.Property(record => record.StoreId).ValueGeneratedNever();
            entity.ToTable("StoreMaster");
            entity.Property(record => record.Name).HasMaxLength(200);
            entity.Property(record => record.Label).HasMaxLength(200);
            entity.Property(record => record.Phone).HasMaxLength(50);
            entity.Property(record => record.EmailAddress).HasMaxLength(256);
            entity.Property(record => record.AddressLine1).HasMaxLength(250);
            entity.Property(record => record.AddressLine2).HasMaxLength(250);
            entity.Property(record => record.City).HasMaxLength(150);
            entity.Property(record => record.County).HasMaxLength(150);
            entity.Property(record => record.CountryCode).HasMaxLength(10);
            entity.Property(record => record.PostalCode).HasMaxLength(20);
        });

        modelBuilder.Entity<StoreSlotEntity>(entity =>
        {
            entity.ToTable("StoreSlot");
            entity.HasKey(record => record.StoreSlotId);
            entity.Property(record => record.DayCode).HasMaxLength(3);
            entity.Property(record => record.StoreId).HasMaxLength(20);
            entity.HasIndex(record => new { record.StoreId, record.DayCode, record.IsActive });
        });

        modelBuilder.Entity<StoreManagerClosureEntity>(entity =>
        {
            entity.ToTable("StoreManagerClosures");
            entity.HasKey(record => record.ClosureId);
            entity.Property(record => record.ClosureId).HasMaxLength(50);
            entity.Property(record => record.StoreId).HasMaxLength(20);
            entity.Property(record => record.ClosureType).HasMaxLength(50);
            entity.Property(record => record.Note).HasMaxLength(500);
            entity.HasIndex(record => new { record.StoreId, record.ClosureDate, record.IsActive });
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("StoreAppointment");
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Id).HasMaxLength(20);
            entity.Property(record => record.AppointmentType).HasConversion<string>().HasMaxLength(20);
            entity.Property(record => record.AppointmentReason).HasMaxLength(200);
            entity.Property(record => record.BookingReference).HasMaxLength(100);
            entity.Property(record => record.LocationId).HasMaxLength(20);
            entity.Property(record => record.VisitPurpose).HasMaxLength(100);
            entity.Property(record => record.VisitPurposeDetails).HasMaxLength(500);
            entity.Property(record => record.AdditionalDetails).HasMaxLength(2000);
            entity.Property(record => record.AllocationId).HasMaxLength(100);
            entity.Property(record => record.AppointmentStatus).HasMaxLength(20);
            entity.HasIndex(record => new { record.LocationId, record.Date });
        });

        modelBuilder.Entity<StoreAppointmentReferenceResult>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<StoreSlotResult>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<StoreCalendarAvailabilityResultRow>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<StoreManagerStoreRow>(entity => { entity.HasNoKey(); });
        modelBuilder.Entity<StoreManagerOpeningHoursRow>(entity => { entity.HasNoKey(); });
        modelBuilder.Entity<StoreManagerSlotRuleRow>(entity => { entity.HasNoKey(); });
        modelBuilder.Entity<StoreManagerSlotRow>(entity => { entity.HasNoKey(); });
        modelBuilder.Entity<StoreManagerSlotAllocationRow>(entity => { entity.HasNoKey(); });
        modelBuilder.Entity<StoreManagerClosureRow>(entity => { entity.HasNoKey(); });
        modelBuilder.Entity<StoreManagerBookingRow>(entity => { entity.HasNoKey(); });
        modelBuilder.Entity<StoreManagerBookingConflictRow>(entity => { entity.HasNoKey(); });
    }
    private Task<List<TEntity>> ExecuteStoredProcedureInMemoryAsync<TEntity>(string storedProcedureName, IReadOnlyCollection<SqlParameter> parameters)
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(StoreEntity)
            && (storedProcedureName == StoredProcedures.StoreMasterGetAll
                || storedProcedureName == StoredProcedures.StoreMasterGetById))
        {
            var stores = Stores.AsNoTracking();

            if (storedProcedureName == StoredProcedures.StoreMasterGetById)
            {
                var storeId = parameters.FirstOrDefault(parameter => parameter.ParameterName == "@StoreId")?.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(storeId))
                {
                    stores = stores.Where(store => store.StoreId == storeId.Trim());
                }
            }

            return Task.FromResult(stores.Cast<TEntity>().ToList());
        }

        if (typeof(TEntity) == typeof(StoreManagerStoreRow)
            && (storedProcedureName == StoredProcedures.StoreManagerGetStores
                || storedProcedureName == StoredProcedures.StoreManagerGetStoreById))
        {
            var stores = Stores.AsNoTracking();

            if (storedProcedureName == StoredProcedures.StoreManagerGetStoreById)
            {
                var storeId = parameters.FirstOrDefault(parameter => parameter.ParameterName == "@StoreId")?.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(storeId))
                {
                    stores = stores.Where(store => store.StoreId == storeId.Trim());
                }
            }

            var rows = stores.Select(store => new StoreManagerStoreRow
            {
                StoreId = store.StoreId,
                Name = store.Name,
                Label = store.Label,
                AddressLine1 = store.AddressLine1,
                AddressLine2 = store.AddressLine2,
                City = store.City,
                County = store.County,
                PostalCode = store.PostalCode,
                CountryCode = store.CountryCode,
                Phone = store.Phone,
                EmailAddress = store.EmailAddress,
                StoreServicesJson = store.StoreServicesJson,
                MarkedAsClosed = store.MarkedAsClosed
            }).ToList();

            return Task.FromResult(rows.Cast<TEntity>().ToList());
        }

        if (typeof(TEntity) == typeof(StoreSlotResult)
            && storedProcedureName == StoredProcedures.StoreSlotsGetByDate)
        {
            var storeId = parameters.FirstOrDefault(parameter => parameter.ParameterName == "@StoreId")?.Value?.ToString()?.Trim();
            var selectedDate = GetDateOnlyParameter(parameters, "@SelectedDate");
            if (string.IsNullOrWhiteSpace(storeId) || selectedDate is null)
            {
                return Task.FromResult(new List<TEntity>());
            }

            var dayCode = selectedDate.Value.DayOfWeek.ToString()[..3].ToUpperInvariant();
            if (!TryGetStoreOpeningWindow(storeId, selectedDate.Value, out var openingTime, out var closingTime))
            {
                return Task.FromResult(new List<TEntity>());
            }

            var slotResults = StoreSlots
                .AsNoTracking()
                .Where(slot =>
                    slot.StoreId == storeId
                    && slot.IsActive
                    && string.Equals(slot.DayCode, dayCode, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable()
                .Where(slot => IsSlotInsideOpeningHours(slot, openingTime, closingTime))
                .Select(slot => new StoreSlotResult
                {
                    StoreId = slot.StoreId,
                    StartTime = slot.SlotStartTime,
                    EndTime = slot.SlotEndTime,
                    IsAvailable = HasAvailableCapacity(slot, selectedDate.Value)
                })
                .ToList();

            return Task.FromResult(slotResults.Cast<TEntity>().ToList());
        }

        if (typeof(TEntity) == typeof(StoreCalendarAvailabilityResultRow)
            && storedProcedureName == StoredProcedures.StoreCalendarAvailabilityGetByMonth)
        {
            var storeId = parameters.FirstOrDefault(parameter => parameter.ParameterName == "@StoreId")?.Value?.ToString()?.Trim();
            var year = GetParameterValue<int>(parameters, "@Year");
            var month = GetParameterValue<int>(parameters, "@Month");
            if (string.IsNullOrWhiteSpace(storeId) || year < 1 || month is < 1 or > 12)
            {
                return Task.FromResult(new List<TEntity>());
            }

            var firstDate = new DateOnly(year, month, 1);
            var lastDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
            var todayUtc = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
            var startDate = firstDate < todayUtc ? todayUtc : firstDate;

            if (startDate > lastDate)
            {
                return Task.FromResult(new List<TEntity>());
            }

            var calendarResults = Enumerable.Range(startDate.Day, lastDate.Day - startDate.Day + 1)
                .Select(day => new DateOnly(year, month, day))
                .Select(date =>
                {
                    var hasOpeningWindow = TryGetStoreOpeningWindow(storeId, date, out var openingTime, out var closingTime);

                    return new StoreCalendarAvailabilityResultRow
                    {
                        StoreId = storeId,
                        Year = year,
                        Month = month,
                        Date = date,
                        HasAvailableSlot = hasOpeningWindow
                            && StoreSlots
                            .AsNoTracking()
                            .Where(slot =>
                                slot.StoreId == storeId
                                && slot.IsActive
                                && string.Equals(
                                    slot.DayCode,
                                    date.DayOfWeek.ToString().Substring(0, 3).ToUpperInvariant(),
                                    StringComparison.OrdinalIgnoreCase))
                            .AsEnumerable()
                            .Any(slot => IsSlotInsideOpeningHours(slot, openingTime, closingTime) && HasAvailableCapacity(slot, date))
                    };
                })
                .ToList();

            return Task.FromResult(calendarResults.Cast<TEntity>().ToList());
        }

        if (typeof(TEntity) == typeof(StoreAppointmentReferenceResult)
            && storedProcedureName == StoredProcedures.StoreAppointmentGetLatestReferenceId)
        {
            var storeInitials = parameters.FirstOrDefault(parameter => parameter.ParameterName == "@StoreInitials")?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(storeInitials))
            {
                return Task.FromResult(new List<TEntity>());
            }

            var latestReference = StoreAppointments
                .AsNoTracking()
                .Where(appointment => appointment.Id.StartsWith($"BR-{storeInitials}-"))
                .OrderByDescending(appointment => appointment.Id)
                .Select(appointment => appointment.Id)
                .FirstOrDefault();

            var results = string.IsNullOrWhiteSpace(latestReference)
                ? new List<StoreAppointmentReferenceResult>()
                : new List<StoreAppointmentReferenceResult> { new() { Id = latestReference } };

            return Task.FromResult(results.Cast<TEntity>().ToList());
        }

        return Task.FromResult(new List<TEntity>());
    }

    private bool TryGetStoreOpeningWindow(string storeId, DateOnly date, out TimeOnly openingTime, out TimeOnly closingTime)
    {
        openingTime = default;
        closingTime = default;

        var store = Stores.AsNoTracking().FirstOrDefault(item => item.StoreId == storeId);
        if (store is null || store.MarkedAsClosed)
        {
            return false;
        }

        if (StoreManagerClosures
            .AsNoTracking()
            .Any(closure =>
                closure.StoreId == storeId
                && closure.ClosureDate == date
                && closure.IsActive))
        {
            return false;
        }

        return TryGetOpeningWindow(store.StoreWorkHoursJson, date.DayOfWeek, out openingTime, out closingTime);
    }

    private static bool IsSlotInsideOpeningHours(StoreSlotEntity slot, TimeOnly openingTime, TimeOnly closingTime)
    {
        return slot.SlotStartTime >= openingTime
            && slot.SlotEndTime <= closingTime;
    }

    private static bool TryGetOpeningWindow(string? storeWorkHoursJson, DayOfWeek dayOfWeek, out TimeOnly openingTime, out TimeOnly closingTime)
    {
        openingTime = default;
        closingTime = default;

        if (string.IsNullOrWhiteSpace(storeWorkHoursJson))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(storeWorkHoursJson);
            var hours = document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.EnumerateArray()
                : document.RootElement.TryGetProperty("weeklyHours", out var weeklyHours) && weeklyHours.ValueKind == JsonValueKind.Array
                    ? weeklyHours.EnumerateArray()
                    : Enumerable.Empty<JsonElement>();

            foreach (var workHour in hours)
            {
                if (!TryGetString(workHour, "day", out var day)
                    || !IsMatchingDay(day, dayOfWeek))
                {
                    continue;
                }

                if (TryGetBoolean(workHour, "isClosed")
                    || !TryGetString(workHour, "openingTime", out var openingTimeValue)
                    || !TryGetString(workHour, "closingTime", out var closingTimeValue)
                    || !TimeOnly.TryParse(openingTimeValue, out openingTime)
                    || !TimeOnly.TryParse(closingTimeValue, out closingTime)
                    || openingTime >= closingTime)
                {
                    return false;
                }

                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    private bool HasAvailableCapacity(StoreSlotEntity slot, DateOnly date)
    {
        var overlappingAppointments = StoreAppointments
            .AsNoTracking()
            .Where(appointment =>
                appointment.LocationId == slot.StoreId
                && appointment.Date == date
                && appointment.SlotStart < slot.SlotEndTime
                && appointment.SlotEnd > slot.SlotStartTime
                && appointment.AppointmentStatus == Constants.Constants.BookedAppointmentStatus)
            .ToList();

        if (overlappingAppointments.Any(IsStoreManagerFullSlotBlockedAppointment))
        {
            return false;
        }

        var appointmentCount = overlappingAppointments.Count;
        return slot.Capacity - appointmentCount > 0;
    }

    private static bool IsStoreManagerFullSlotBlockedAppointment(Appointment appointment)
    {
        return string.Equals(appointment.AppointmentReason, Constants.Constants.StoreManagerBlockedAppointmentReason, StringComparison.OrdinalIgnoreCase)
            || (string.Equals(appointment.VisitPurpose, Constants.Constants.StoreManagerVisitPurpose, StringComparison.OrdinalIgnoreCase)
                && string.Equals(appointment.VisitPurposeDetails, Constants.Constants.StoreManagerBlockedVisitPurposeDetails, StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(appointment.AllocationId)
                && !HasStoreManagerAllocationIndex(appointment.AdditionalDetails));
    }

    private static bool HasStoreManagerAllocationIndex(string? additionalDetails)
    {
        if (string.IsNullOrWhiteSpace(additionalDetails))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(additionalDetails);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("allocationIndex", out var allocationIndex)
                && allocationIndex.ValueKind == JsonValueKind.Number
                && allocationIndex.TryGetInt32(out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsMatchingDay(string day, DayOfWeek dayOfWeek)
    {
        var fullDay = dayOfWeek.ToString();
        return string.Equals(day.Trim(), fullDay, StringComparison.OrdinalIgnoreCase)
            || string.Equals(day.Trim(), fullDay[..3], StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        value = property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : property.ToString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryGetBoolean(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => property.TryGetInt32(out var value) && value == 1,
            JsonValueKind.String => IsTruthyString(property.GetString()),
            _ => false
        };
    }

    private static bool IsTruthyString(string? value)
    {
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
    }

    private static DateOnly? GetDateOnlyParameter(IReadOnlyCollection<SqlParameter> parameters, string parameterName)
    {
        var rawDate = parameters.FirstOrDefault(item => item.ParameterName == parameterName)?.Value;
        return rawDate switch
        {
            null or DBNull => null,
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => DateOnly.TryParse(rawDate.ToString(), out var parsedDate) ? parsedDate : null
        };
    }

    private async Task<int> ExecuteStoredProcedureInMemoryAsync(string storedProcedureName, IReadOnlyCollection<SqlParameter> parameters)
    {
        if (storedProcedureName == StoredProcedures.StoreManagerUpdateStore)
        {
            var storeId = GetParameterValue<string>(parameters, "@StoreId")?.Trim();
            if (string.IsNullOrWhiteSpace(storeId))
            {
                return 0;
            }

            var store = await Stores.FirstOrDefaultAsync(item => item.StoreId == storeId);
            if (store is null)
            {
                return 0;
            }

            store.Name = GetParameterValue<string>(parameters, "@Name") ?? store.Name;
            store.AddressLine1 = GetParameterValue<string>(parameters, "@AddressLine1") ?? store.AddressLine1;
            store.AddressLine2 = GetParameterValue<string>(parameters, "@AddressLine2") ?? store.AddressLine2;
            store.City = GetParameterValue<string>(parameters, "@City") ?? store.City;
            store.County = GetParameterValue<string>(parameters, "@County") ?? store.County;
            store.PostalCode = GetParameterValue<string>(parameters, "@PostalCode") ?? store.PostalCode;
            store.CountryCode = GetParameterValue<string>(parameters, "@CountryCode") ?? store.CountryCode;
            store.Phone = GetParameterValue<string>(parameters, "@Telephone") ?? store.Phone;
            store.EmailAddress = GetParameterValue<string>(parameters, "@EmailAddress") ?? store.EmailAddress;
            store.StoreServicesJson = GetParameterValue<string>(parameters, "@StoreServicesJson") ?? store.StoreServicesJson;
            ApplyOptionalBooleanParameter(parameters, "@MarkedAsClosed", value => store.MarkedAsClosed = value);

            return await SaveChangesAsync();
        }

        if (storedProcedureName != StoredProcedures.StoreAppointmentSave)
        {
            return 0;
        }

        var appointment = new Appointment
        {
            Id = GetParameterValue<string>(parameters, "@Id") ?? string.Empty,
            AppointmentType = Enum.TryParse<AppointmentType>(GetParameterValue<string>(parameters, "@AppointmentType"), out var appointmentType)
                ? appointmentType
                : AppointmentType.InStore,
            AppointmentReason = GetParameterValue<string>(parameters, "@AppointmentReason") ?? string.Empty,
            BookingReference = GetParameterValue<string>(parameters, "@BookingReference") ?? string.Empty,
            Date = GetParameterValue<DateOnly>(parameters, "@Date"),
            SlotStart = GetParameterValue<TimeOnly>(parameters, "@SlotStart"),
            SlotEnd = GetParameterValue<TimeOnly>(parameters, "@SlotEnd"),
            LocationId = GetParameterValue<string>(parameters, "@LocationId") ?? string.Empty,
            VisitPurpose = GetParameterValue<string>(parameters, "@VisitPurpose") ?? string.Empty,
            VisitPurposeDetails = GetParameterValue<string>(parameters, "@VisitPurposeDetails") ?? string.Empty,
            AdditionalDetails = GetParameterValue<string>(parameters, "@AdditionalDetails") ?? string.Empty
        };

        StoreAppointments.Add(appointment);
        return await SaveChangesAsync();
    }

    private static void ApplyOptionalBooleanParameter(IReadOnlyCollection<SqlParameter> parameters, string parameterName, Action<bool> apply)
    {
        var parameter = parameters.FirstOrDefault(item => item.ParameterName == parameterName);
        if (parameter?.Value is null or DBNull)
        {
            return;
        }

        apply(Convert.ToBoolean(parameter.Value));
    }

    private static T? GetParameterValue<T>(IReadOnlyCollection<SqlParameter> parameters, string parameterName)
    {
        var parameter = parameters.FirstOrDefault(item => item.ParameterName == parameterName);
        if (parameter?.Value is null or DBNull)
        {
            return default;
        }

        if (parameter.Value is T typedValue)
        {
            return typedValue;
        }

        return (T?)Convert.ChangeType(parameter.Value, typeof(T));
    }

    private static string BuildStoredProcedureSql(string storedProcedureName, IReadOnlyCollection<SqlParameter> parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedureName);

        if (parameters.Count == 0)
        {
            return $"EXEC {storedProcedureName}";
        }

        var parameterList = string.Join(", ", parameters.Select(parameter =>
            parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput
                ? $"{parameter.ParameterName} OUTPUT"
                : parameter.ParameterName));
        return $"EXEC {storedProcedureName} {parameterList}";
    }
}
