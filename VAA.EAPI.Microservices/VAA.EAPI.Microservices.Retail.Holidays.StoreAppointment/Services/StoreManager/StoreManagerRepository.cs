using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager;

public sealed class StoreManagerRepository : IStoreManagerService
{
    private const string StoreManagerKidsArea = "Kids area";
    private const string StoreManagerWelcomeDrinksAndSnacks = "Welcome drinks and snacks";
    private const string StoreManagerAircraftSeats = "Aircraft seats";
    private const string StoreManagerSecretCinema = "Secret cinema";
    private static readonly Regex UkTelephoneRegex = new(@"^0\d{10}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex EmailRegex = new(@"^[A-Za-z0-9](?:[A-Za-z0-9._%+-]{0,62}[A-Za-z0-9])?@(?:[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,63}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly StoreAppointmentDbContext _dbContext;
    private readonly StoreManagerScheduleRepository _scheduleRepository;
    private readonly StoreManagerClosureRepository _closureRepository;

    public StoreManagerRepository(StoreAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
        _scheduleRepository = new StoreManagerScheduleRepository(dbContext);
        _closureRepository = new StoreManagerClosureRepository(dbContext);
    }

    public async Task<IReadOnlyCollection<StoreResponse>> GetStores()
    {
        var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerStoreRow>(StoredProcedures.StoreManagerGetStores);
        return rows.Select(MapStore).ToArray();
    }

    public async Task<ServiceResult<StoreResponse>> GetStore(string id)
    {
        var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerStoreRow>(
            StoredProcedures.StoreManagerGetStoreById,
            new SqlParameter("@StoreId", id));
        var store = rows.Select(MapStore).FirstOrDefault();
        return store is null ? ServiceResult<StoreResponse>.NotFound($"Store '{id}' was not found.") : ServiceResult<StoreResponse>.Success(store);
    }

    public async Task<ServiceResult<StoreResponse>> UpdateStore(string id, StoreRequest request)
    {
        var storeId = id.Trim();
        var emailAddress = NormalizeOptionalEmail(request.Email);
        var validationError = ValidateStoreRequest(request);
        if (validationError is not null)
        {
            return ServiceResult<StoreResponse>.ValidationError(validationError);
        }

        var wasClosed = await _dbContext.Stores
            .Where(store => store.StoreId == storeId)
            .Select(store => store.MarkedAsClosed)
            .FirstOrDefaultAsync();

        if (_dbContext.Database.IsRelational())
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            if (request.IsClosed)
            {
                var conflicts = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerBookingConflictRow>(
                    StoredProcedures.StoreManagerValidateStoreClosedBookingImpact,
                    new SqlParameter("@StoreId", storeId));
                var conflictResult = StoreManagerBookingConflict.Create<StoreResponse>(storeId, conflicts);
                if (conflictResult is not null)
                {
                    return conflictResult;
                }
            }

            await UpdateStoreAndSlots(storeId, request, emailAddress, wasClosed);
            await transaction.CommitAsync();
        }
        else
        {
            await UpdateStoreAndSlots(storeId, request, emailAddress, wasClosed);
        }

        return await GetStore(storeId);
    }

    private async Task UpdateStoreAndSlots(string storeId, StoreRequest request, string? emailAddress, bool wasClosed)
    {
        await _dbContext.ExecuteStoredProcedureAsync(
            StoredProcedures.StoreManagerUpdateStore,
            new SqlParameter("@StoreId", storeId),
            new SqlParameter("@Name", request.Name),
            new SqlParameter("@AddressLine1", request.AddressLine1),
            new SqlParameter("@AddressLine2", (object?)request.AddressLine2 ?? DBNull.Value),
            new SqlParameter("@City", request.CityTown),
            new SqlParameter("@County", (object?)request.CountyRegion ?? DBNull.Value),
            new SqlParameter("@PostalCode", request.PostalCode),
            new SqlParameter("@CountryCode", "GB"),
            new SqlParameter("@Telephone", (object?)request.Telephone ?? DBNull.Value),
            new SqlParameter("@EmailAddress", (object?)emailAddress ?? DBNull.Value),
            new SqlParameter("@MarkedAsClosed", request.IsClosed),
            new SqlParameter("@StoreServicesJson", SerializeServices(request.Services)));

        if (request.IsClosed)
        {
            await DeactivateStoreSlots(storeId);
        }
        else if (wasClosed)
        {
            await _scheduleRepository.GenerateSlots(storeId);
        }
    }

    public Task<ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>> GetOpeningHours(string storeId)
        => _scheduleRepository.GetOpeningHours(storeId);

    public Task<ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>> ReplaceOpeningHours(string storeId, OpeningHoursReplaceRequest request)
        => _scheduleRepository.ReplaceOpeningHours(storeId, request);

    public Task<ServiceResult<IReadOnlyCollection<SlotRuleResponse>>> GetSlotRules(string storeId)
        => _scheduleRepository.GetSlotRules(storeId);

    public Task<ServiceResult<IReadOnlyCollection<SlotRuleResponse>>> ReplaceSlotRules(string storeId, SlotRuleReplaceRequest request)
        => _scheduleRepository.ReplaceSlotRules(storeId, request);

    public Task<ServiceResult<IReadOnlyCollection<WindowResponse>>> GetWindows(string storeId, DateTimeOffset from, DateTimeOffset to)
        => _scheduleRepository.GetWindows(storeId, from, to);

    public Task<ServiceResult<WindowResponse>> GetSlot(string storeSlotId)
        => _scheduleRepository.GetSlot(storeSlotId);

    public Task<ServiceResult<IReadOnlyCollection<WindowResponse>>> GenerateSlots(string storeId)
        => _scheduleRepository.GenerateSlots(storeId);

    public Task<ServiceResult<WindowResponse>> UpdateSlotStatus(string storeId, string slotId, SlotStatusUpdateRequest request)
        => _scheduleRepository.UpdateSlotStatus(storeId, slotId, request);

    public Task<ServiceResult<SlotAllocationResponse>> GetSlotAllocations(string storeId, string slotId, DateTimeOffset startAt)
        => _scheduleRepository.GetSlotAllocations(storeId, slotId, startAt);

    public Task<ServiceResult<SlotAllocationResponse>> UpdateSlotAllocationStatus(string storeId, string slotId, string allocationId, SlotAllocationStatusUpdateRequest request)
        => _scheduleRepository.UpdateSlotAllocationStatus(storeId, slotId, allocationId, request);

    public Task<ServiceResult<IReadOnlyCollection<AvailabilityWindowResponse>>> GetAvailability(string storeId, DateOnly date)
        => _scheduleRepository.GetAvailability(storeId, date);

    public Task<ServiceResult<IReadOnlyCollection<AvailabilityWindowResponse>>> GetAvailability(string storeId, DateOnly from, DateOnly to)
        => _scheduleRepository.GetAvailability(storeId, from, to);

    public Task<ServiceResult<IReadOnlyCollection<StoreClosureResponse>>> GetClosures(string storeId, DateOnly from, DateOnly to)
        => _closureRepository.GetClosures(storeId, from, to);

    public Task<ServiceResult<StoreClosureResponse>> CreateClosure(string storeId, StoreClosureRequest request)
        => _closureRepository.CreateClosure(storeId, request);

    public Task<ServiceResult<bool>> DeleteClosure(string storeId, string closureId)
        => _closureRepository.DeleteClosure(storeId, closureId);

    public Task<ServiceResult<IReadOnlyCollection<ManageBookingResponse>>> GetBookings(string storeId, DateOnly date, string? status, string? query)
        => _closureRepository.GetBookings(storeId, date, status, query);

    public Task<ServiceResult<ManageBookingResponse>> CancelBooking(string storeId, string bookingId, BookingCancellationRequest request)
        => _closureRepository.CancelBooking(storeId, bookingId, request);

    private static StoreResponse MapStore(StoreManagerStoreRow row) => new()
    {
        Id = ParseString(row.StoreId),
        Name = row.Name ?? string.Empty,
        Label = row.Label ?? string.Empty,
        AddressLine1 = row.AddressLine1 ?? string.Empty,
        AddressLine2 = row.AddressLine2 ?? string.Empty,
        CityTown = row.City ?? string.Empty,
        CountyRegion = row.County ?? string.Empty,
        PostalCode = row.PostalCode ?? string.Empty,
        Phone = row.Phone ?? string.Empty,
        Email = row.EmailAddress ?? string.Empty,
        Services = GetStoreServices(row),
        IsClosed = row.MarkedAsClosed
    };

    private static object SerializeServices(IEnumerable<string>? services)
    {
        return services is null
            ? DBNull.Value
            : JsonConvert.SerializeObject(NormalizeServices(services));
    }

    private static IReadOnlyCollection<string> GetStoreServices(StoreManagerStoreRow row)
    {
        if (string.IsNullOrWhiteSpace(row.StoreServicesJson))
        {
            return Array.Empty<string>();
        }

        try
        {
            return NormalizeServices(JsonConvert.DeserializeObject<string[]>(row.StoreServicesJson) ?? []);
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static string[] NormalizeServices(IEnumerable<string> services)
    {
        return services
            .Where(service => !string.IsNullOrWhiteSpace(service))
            .Select(service => NormalizeServiceName(service.Trim()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
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

    private static string? ValidateStoreRequest(StoreRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Telephone) && !UkTelephoneRegex.IsMatch(request.Telephone))
        {
            return "Telephone must be a valid UK number with no spaces.";
        }

        var emailAddress = NormalizeOptionalEmail(request.Email);
        if (emailAddress is not null && !IsValidEmailAddress(emailAddress))
        {
            return "Email must be a valid email address.";
        }

        return null;
    }

    private static bool IsValidEmailAddress(string email)
    {
        return EmailRegex.IsMatch(email) && !email.Contains("..", StringComparison.Ordinal);
    }

    private static string? NormalizeOptionalEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    private async Task DeactivateStoreSlots(string storeId)
    {
        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.StoreSlots
                .Where(slot => slot.StoreId == storeId && slot.IsActive)
                .ExecuteUpdateAsync(setters => setters.SetProperty(slot => slot.IsActive, false));
            return;
        }

        var slots = await _dbContext.StoreSlots
            .Where(slot => slot.StoreId == storeId && slot.IsActive)
            .ToListAsync();

        foreach (var slot in slots)
        {
            slot.IsActive = false;
        }

        if (slots.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
        }
    }

    private static string ParseString(string? raw) => raw?.Trim() ?? string.Empty;
}
