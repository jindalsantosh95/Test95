using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Store;

public class StoreRepository : IStoreRepository
{
    private const int StoreClosureSqlInClauseThreshold = 2000;
    private const string StoreManagerKidsArea = "Kids area";
    private const string StoreManagerWelcomeDrinksAndSnacks = "Welcome drinks and snacks";
    private const string StoreManagerAircraftSeats = "Aircraft seats";
    private const string StoreManagerSecretCinema = "Secret cinema";
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly StoreAppointmentDbContext _dbContext;
    private readonly string _defaultStoreImageSrc;
    private readonly string _defaultStoreImageAlt;

    public StoreRepository(StoreAppointmentDbContext dbContext, IConfiguration? configuration = null)
    {
        _dbContext = dbContext;
        _defaultStoreImageSrc = configuration?[Constants.Environment.StoreDefaultImageSrc] ?? string.Empty;
        _defaultStoreImageAlt = configuration?[Constants.Environment.StoreDefaultImageAlt] ?? string.Empty;
    }

    public async Task<IReadOnlyCollection<StoreResult>> GetStoresAsync()
    {
        var stores = await _dbContext.ExecuteStoredProcedureAsync<Services.Entities.StoreEntity>(StoredProcedures.StoreMasterGetAll);
        var storesClosedToday = await GetStoresClosedTodayAsync(stores.Select(store => store.StoreId));
        return stores
            .Select(store => MapStoreEntity(store, storesClosedToday.Contains(store.StoreId)))
            .ToList();
    }

    public async Task<StoreResult?> GetStoreByIdAsync(string storeId)
    {
        var stores = await _dbContext.ExecuteStoredProcedureAsync<Services.Entities.StoreEntity>(
            StoredProcedures.StoreMasterGetById,
            new SqlParameter("@StoreId", storeId.Trim()));

        var store = stores.FirstOrDefault();
        if (store is null)
        {
            return null;
        }

        var storesClosedToday = await GetStoresClosedTodayAsync(new[] { store.StoreId });
        return MapStoreEntity(store, storesClosedToday.Contains(store.StoreId));
    }

    public async Task<IReadOnlyCollection<StoreSlot>> GetStoreSlotsByDateAsync(string storeId, DateTime selectedDate)
    {
        var requestedDate = selectedDate.Date;
        var ukNow = BritishTimeZoneHelper.ConvertToUkLocal(DateTimeOffset.UtcNow);
        var isTodayInUk = requestedDate == ukNow.Date;
        var availabilityRules = await GetStoreAvailabilityRulesAsync(storeId);

        var slots = await GetFilteredStoreSlotResultsByDateAsync(storeId, requestedDate, availabilityRules);

        return slots
           .Select(record =>
           {
               var slotStart = BritishTimeZoneHelper.BuildStoreLocalDateTimeOffset(requestedDate, record.StartTime);
               var slotEnd = BritishTimeZoneHelper.BuildStoreLocalDateTimeOffset(requestedDate, record.EndTime);

               return new StoreSlot
               {
                   StartTime = slotStart,
                   EndTime = slotEnd,
                   IsAvailable = record.IsAvailable && (!isTodayInUk || slotStart >= ukNow)
               };
           })
            .ToList();
    }

    public async Task<IReadOnlyCollection<StoreDayAvailability>> GetStoreCalendarAvailabilityAsync(string storeId, int year, int month)
    {
        if (month is < 1 or > 12 || year < 1)
        {
            return Array.Empty<StoreDayAvailability>();
        }

        var days = await _dbContext.ExecuteStoredProcedureAsync<StoreCalendarAvailabilityResultRow>(
            StoredProcedures.StoreCalendarAvailabilityGetByMonth,
            new SqlParameter("@StoreId", storeId.Trim()),
            new SqlParameter("@Year", year),
            new SqlParameter("@Month", month));

        return days
            .Select(record => new StoreDayAvailability
            {
                Date = record.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                HasAvailableSlot = record.HasAvailableSlot
            })
            .ToList();
    }

    private async Task<IReadOnlyCollection<StoreSlotResult>> GetFilteredStoreSlotResultsByDateAsync(
        string storeId,
        DateTime requestedDate,
        StoreAvailabilityRules availabilityRules)
    {
        if (!TryGetOpeningWindow(requestedDate, availabilityRules, out var openingTime, out var closingTime))
        {
            return Array.Empty<StoreSlotResult>();
        }

        var slots = await _dbContext.ExecuteStoredProcedureAsync<StoreSlotResult>(
            StoredProcedures.StoreSlotsGetByDate,
            new SqlParameter("@StoreId", storeId.Trim()),
            new SqlParameter("@SelectedDate", SqlDbType.Date)
            {
                Value = requestedDate.Date
            });

        return slots
            .Where(slot => IsSlotInsideOpeningHours(slot.StartTime, slot.EndTime, openingTime, closingTime))
            .ToList();
    }

    private async Task<StoreAvailabilityRules> GetStoreAvailabilityRulesAsync(string storeId)
    {
        var store = await _dbContext.Stores
            .AsNoTracking()
            .Where(store => store.StoreId == storeId.Trim())
            .Select(store => new
            {
                store.MarkedAsClosed,
                store.StoreWorkHoursJson
            })
            .FirstOrDefaultAsync();

        if (store is null)
        {
            return new StoreAvailabilityRules(false, Array.Empty<StoreWorkHours>());
        }

        return new StoreAvailabilityRules(
            store.MarkedAsClosed,
            ParseWorkHoursPayload(store.StoreWorkHoursJson ?? string.Empty).WeeklyHours);
    }

    private static bool IsSlotInsideOpeningHours(TimeOnly slotStart, TimeOnly slotEnd, TimeOnly openingTime, TimeOnly closingTime)
    {
        return slotStart >= openingTime && slotEnd <= closingTime;
    }

    private static bool TryGetOpeningWindow(
        DateTime requestedDate,
        StoreAvailabilityRules availabilityRules,
        out TimeOnly openingTime,
        out TimeOnly closingTime)
    {
        openingTime = default;
        closingTime = default;

        if (availabilityRules.IsStoreClosed)
        {
            return false;
        }

        var workHour = availabilityRules.WorkHours.FirstOrDefault(item => IsMatchingDay(item.Day, requestedDate.DayOfWeek));
        if (workHour is null
            || workHour.IsClosed
            || !TimeOnly.TryParse(workHour.OpeningTime, out openingTime)
            || !TimeOnly.TryParse(workHour.ClosingTime, out closingTime)
            || openingTime >= closingTime)
        {
            return false;
        }

        return true;
    }

    private sealed record StoreAvailabilityRules(bool IsStoreClosed, IReadOnlyCollection<StoreWorkHours> WorkHours);

    private async Task<HashSet<string>> GetStoresClosedTodayAsync(IEnumerable<string> storeIds)
    {
        var storeIdSet = storeIds
            .Where(storeId => !string.IsNullOrWhiteSpace(storeId))
            .Select(storeId => storeId.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (storeIdSet.Count == 0)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var todayUk = GetTodayUkDate();

        var closuresQuery = _dbContext.StoreManagerClosures
            .AsNoTracking()
            .Where(closure =>
                closure.ClosureDate == todayUk
                && closure.IsActive);

        if (storeIdSet.Count <= StoreClosureSqlInClauseThreshold)
        {
            closuresQuery = closuresQuery.Where(closure => storeIdSet.Contains(closure.StoreId));
        }

        var closedStoreIds = await closuresQuery
            .Select(closure => closure.StoreId)
            .Distinct()
            .ToListAsync();

        return closedStoreIds
            .Where(storeIdSet.Contains)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private StoreResult MapStoreEntity(Services.Entities.StoreEntity store, bool isClosedToday)
    {
        var workHoursPayload = ParseWorkHoursPayload(store.StoreWorkHoursJson);
        var specialHours = ParseSpecialHours(store.StoreSpecialHoursJson, workHoursPayload.SpecialHours);
        var workHours = workHoursPayload.WeeklyHours.ToList();
        var storeImage = BuildStoreImage(store);

        return new StoreResult
        {
            StoreId = store.StoreId,
            Name = store.Name,
            Label = store.Label,
            Latitude = store.Latitude,
            Longitude = store.Longitude,
            Distance = string.Empty,
            DrivingTime = string.Empty,
            MarkedAsClosed = store.MarkedAsClosed,
            IsClosest = false,
            IsNew = false,
            FeaturedImage = storeImage,
            CardImages = Enumerable.Empty<StoreImage>(),
            FullDetails = new StoreInformation
            {
                Details = new StoreContactDetails
                {
                    Address = BuildAddress(store),
                    Phone = store.Phone,
                    EmailAddress = store.EmailAddress
                },
                OpeningHours = new StoreOpeningHours
                {
                    IsOpenNow = !isClosedToday && IsOpenNow(workHours),
                    WeeklyHours = workHours
                        .Select(workHour => new StoreOpeningWindow
                        {
                            Day = workHour.Day,
                            Hours = FormatWeeklyHours(workHour)
                        })
                        .ToList(),
                    SpecialHours = specialHours
                        .Select(MapSpecialHour)
                        .Where(specialHour => specialHour.Date != DateOnly.MinValue)
                        .ToList()
                },
                Services = new StoreServiceList
                {
                    InStoreFacilities = BuildInStoreFacilities(store)
                },
            },
            StoreWorkHours = workHours
        };
    }
    private StoreImage BuildStoreImage(Services.Entities.StoreEntity store)
    {
        var src = string.IsNullOrWhiteSpace(store.StoreImageSrc) ? _defaultStoreImageSrc : store.StoreImageSrc;
        var alt = string.IsNullOrWhiteSpace(store.StoreImageAlt) ? _defaultStoreImageAlt : store.StoreImageAlt;

        return new StoreImage
        {
            Src = src,
            Alt = alt
        };
    }

    private static bool IsOpenNow(IEnumerable<StoreWorkHours> workHours)
    {
        var workHourList = workHours.ToList();
        if (workHourList.Count == 0)
        {
            return false;
        }

        var localNow = BritishTimeZoneHelper.ConvertToUkLocal(DateTimeOffset.UtcNow);
        var currentTime = TimeOnly.FromDateTime(localNow.DateTime);

        var todaysWorkHours = workHourList
            .Where(workHour => IsMatchingDay(workHour.Day, localNow.DayOfWeek))
            .ToList();

        foreach (var workHour in todaysWorkHours)
        {
            if (workHour.IsClosed)
            {
                continue;
            }

            if (!TimeOnly.TryParse(workHour.OpeningTime, out var openingTime)
                || !TimeOnly.TryParse(workHour.ClosingTime, out var closingTime))
            {
                continue;
            }

            if (openingTime <= closingTime)
            {
                if (currentTime >= openingTime && currentTime < closingTime)
                {
                    return true;
                }
            }
            else
            {
                if (currentTime >= openingTime || currentTime < closingTime)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsMatchingDay(string day, DayOfWeek dayOfWeek)
    {
        if (string.IsNullOrWhiteSpace(day))
        {
            return false;
        }

        var fullDay = dayOfWeek.ToString();
        var shortDay = fullDay[..3];

        return string.Equals(day.Trim(), fullDay, StringComparison.OrdinalIgnoreCase)
            || string.Equals(day.Trim(), shortDay, StringComparison.OrdinalIgnoreCase);
    }

    private static DateOnly GetTodayUkDate()
    {
        return DateOnly.FromDateTime(BritishTimeZoneHelper.ConvertToUkLocal(DateTimeOffset.UtcNow).DateTime);
    }

    private static StoreExceptionalOpening MapSpecialHour(StoreSpecialHours specialHour)
    {
        return new StoreExceptionalOpening
        {
            Label = string.IsNullOrWhiteSpace(specialHour.Label)
                ? specialHour.IsClosed ? "Closed" : "Special hours"
                : specialHour.Label,
            Date = DateOnly.TryParse(specialHour.Date, out var parsedDate)
                ? parsedDate
                : DateOnly.MinValue,
            OpeningTime = specialHour.OpeningTime,
            ClosingTime = specialHour.ClosingTime,
            IsClosed = specialHour.IsClosed
        };
    }
    private static string FormatWeeklyHours(StoreWorkHours workHour)
    {
        if (workHour.IsClosed
            || string.IsNullOrWhiteSpace(workHour.OpeningTime)
            || string.IsNullOrWhiteSpace(workHour.ClosingTime))
        {
            return "Closed";
        }

        return $"{workHour.OpeningTime} - {workHour.ClosingTime}";
    }

    private static IReadOnlyCollection<string> BuildInStoreFacilities(Services.Entities.StoreEntity store)
    {
        if (string.IsNullOrWhiteSpace(store.StoreServicesJson))
        {
            return Array.Empty<string>();
        }

        try
        {
            var services = JsonSerializer.Deserialize<string[]>(store.StoreServicesJson, _jsonOptions) ?? [];
            return services
                .Where(service => !string.IsNullOrWhiteSpace(service))
                .Select(service => NormalizeServiceName(service.Trim()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static string NormalizeServiceName(string serviceName)
    {
        if (serviceName.Equals(Constants.Constants.KidsArea, StringComparison.OrdinalIgnoreCase))
        {
            return StoreManagerKidsArea;
        }

        if (serviceName.Equals(Constants.Constants.WelcomeDrinksAndSnacks, StringComparison.OrdinalIgnoreCase))
        {
            return StoreManagerWelcomeDrinksAndSnacks;
        }

        if (serviceName.Equals(Constants.Constants.AircraftSeats, StringComparison.OrdinalIgnoreCase))
        {
            return StoreManagerAircraftSeats;
        }

        return serviceName.Equals(Constants.Constants.SecretCinema, StringComparison.OrdinalIgnoreCase)
            ? StoreManagerSecretCinema
            : serviceName;
    }

    private static StoreAddress BuildAddress(Services.Entities.StoreEntity store)
    {
        return new StoreAddress
        {
            AddressLine1 = store.AddressLine1 ?? string.Empty,
            AddressLine2 = store.AddressLine2 ?? string.Empty,
            City = store.City ?? string.Empty,
            County = store.County ?? string.Empty,
            CountryCode = store.CountryCode ?? string.Empty,
            PostalCode = store.PostalCode ?? string.Empty
        };
    }

    private static IReadOnlyCollection<StoreSpecialHours> ParseSpecialHours(string raw, IReadOnlyCollection<StoreSpecialHours> fallbackSpecialHours)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallbackSpecialHours;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<StoreSpecialHoursPayload>(raw, _jsonOptions);
            if (payload?.SpecialHours is not null)
            {
                return payload.SpecialHours;
            }
        }
        catch (JsonException)
        {
            // Fall through and attempt legacy list payload parsing.
        }

        try
        {
            var listPayload = JsonSerializer.Deserialize<List<StoreSpecialHours>>(raw, _jsonOptions);
            if (listPayload is not null)
            {
                return listPayload;
            }
        }
        catch (JsonException)
        {
            // Ignore invalid payloads and fallback to legacy embedded special hours.
        }

        return fallbackSpecialHours;
    }

    private static StoreWorkHoursPayload ParseWorkHoursPayload(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new StoreWorkHoursPayload();
        }

        try
        {
            var trimmed = raw.TrimStart();
            if (trimmed.StartsWith("[", StringComparison.Ordinal))
            {
                var weeklyHours = JsonSerializer.Deserialize<List<StoreWorkHours>>(raw, _jsonOptions) ?? new List<StoreWorkHours>();
                return new StoreWorkHoursPayload
                {
                    WeeklyHours = weeklyHours
                };
            }

            return JsonSerializer.Deserialize<StoreWorkHoursPayload>(raw, _jsonOptions) ?? new StoreWorkHoursPayload();
        }
        catch (JsonException)
        {
            return new StoreWorkHoursPayload();
        }
    }
}
