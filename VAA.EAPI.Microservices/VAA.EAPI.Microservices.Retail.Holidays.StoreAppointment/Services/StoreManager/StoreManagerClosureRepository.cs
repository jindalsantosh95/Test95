using Microsoft.Data.SqlClient;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager;

internal sealed class StoreManagerClosureRepository
{
    private readonly StoreAppointmentDbContext _dbContext;

    public StoreManagerClosureRepository(StoreAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<IReadOnlyCollection<StoreClosureResponse>>> GetClosures(string storeId, DateOnly from, DateOnly to)
    {
        var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerClosureRow>(StoredProcedures.StoreManagerGetClosures, new SqlParameter("@StoreId", storeId.ToString()), new SqlParameter("@FromDate", from), new SqlParameter("@ToDate", to));
        return ServiceResult<IReadOnlyCollection<StoreClosureResponse>>.Success(rows.Select(x => new StoreClosureResponse { Id = ParseString(x.ClosureId), ClosureDate = x.ClosureDate, ClosureType = x.ClosureType ?? string.Empty, Reason = x.Note }).ToArray());
    }

    public async Task<ServiceResult<StoreClosureResponse>> CreateClosure(string storeId, StoreClosureRequest request)
    {
        var resultCodeParameter = new SqlParameter("@ResultCode", System.Data.SqlDbType.NVarChar, 50)
        {
            Direction = System.Data.ParameterDirection.Output
        };
        var activeBookingCountParameter = new SqlParameter("@ActiveBookingCount", System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        var closureIdParameter = new SqlParameter("@ClosureId", System.Data.SqlDbType.NVarChar, 50)
        {
            Direction = System.Data.ParameterDirection.InputOutput,
            Value = DBNull.Value
        };

        await _dbContext.ExecuteStoredProcedureAsync(
            StoredProcedures.StoreManagerCreateClosure,
            closureIdParameter,
            new SqlParameter("@StoreId", storeId.ToString()),
            new SqlParameter("@ClosureDate", request.ClosureDate),
            new SqlParameter("@ClosureType", request.ClosureType),
            new SqlParameter("@Note", (object?)request.Reason ?? DBNull.Value),
            resultCodeParameter,
            activeBookingCountParameter);

        var resultCode = ParseString(resultCodeParameter.Value?.ToString());
        if (string.Equals(resultCode, "ACTIVE_BOOKINGS_EXIST", StringComparison.Ordinal))
        {
            var activeBookingCount = activeBookingCountParameter.Value is null or DBNull
                ? 0
                : Convert.ToInt32(activeBookingCountParameter.Value, System.Globalization.CultureInfo.InvariantCulture);

            return ServiceResult<StoreClosureResponse>.Conflict(
                "ACTIVE_BOOKINGS_EXIST",
                "The store cannot be closed while active bookings exist.",
                new Dictionary<string, object?>
                {
                    ["storeId"] = storeId,
                    ["closureDate"] = request.ClosureDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    ["activeBookingCount"] = activeBookingCount
                });
        }

        if (string.Equals(resultCode, "CLOSURE_ALREADY_EXISTS", StringComparison.Ordinal))
        {
            return ServiceResult<StoreClosureResponse>.Conflict(
                "CLOSURE_ALREADY_EXISTS",
                $"Closure already exists for store '{storeId}' on '{request.ClosureDate:yyyy-MM-dd}'.");
        }

        var id = ParseString(closureIdParameter.Value?.ToString());
        if (string.IsNullOrWhiteSpace(id))
        {
            var recoveredClosure = await TryRecoverCreatedClosure(storeId, request);
            if (recoveredClosure is not null)
            {
                return ServiceResult<StoreClosureResponse>.Success(recoveredClosure);
            }

            return ServiceResult<StoreClosureResponse>.Conflict($"Unable to confirm created closure id for store '{storeId}' on '{request.ClosureDate:yyyy-MM-dd}'.");
        }

        return ServiceResult<StoreClosureResponse>.Success(new StoreClosureResponse { Id = id, ClosureDate = request.ClosureDate, ClosureType = request.ClosureType, Reason = request.Reason });
    }

    public async Task<ServiceResult<bool>> DeleteClosure(string storeId, string closureId)
    {
        var deletedParameter = new SqlParameter("@Deleted", System.Data.SqlDbType.Bit)
        {
            Direction = System.Data.ParameterDirection.Output,
            Value = false
        };

        try
        {
            await _dbContext.ExecuteStoredProcedureAsync(
                StoredProcedures.StoreManagerDeleteClosure,
                new SqlParameter("@StoreId", storeId),
                new SqlParameter("@ClosureId", closureId),
                deletedParameter);

            return deletedParameter.Value is not DBNull && Convert.ToBoolean(deletedParameter.Value)
                ? ServiceResult<bool>.Success(true)
                : ServiceResult<bool>.NotFound($"Closure '{closureId}' was not found for store '{storeId}'.");
        }
        catch (SqlException ex) when (IsDeleteClosureConflict(ex))
        {
            return ServiceResult<bool>.Conflict($"Closure '{closureId}' cannot be deleted because it has existing bookings.");
        }
    }

    public async Task<ServiceResult<IReadOnlyCollection<ManageBookingResponse>>> GetBookings(string storeId, DateOnly date, string? status, string? query)
    {
        var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerBookingRow>(StoredProcedures.StoreManagerGetBookings,
            new SqlParameter("@StoreId", storeId),
            new SqlParameter("@Date", date),
            new SqlParameter("@Status", string.IsNullOrWhiteSpace(status) ? DBNull.Value : status.Trim()),
            new SqlParameter("@Query", string.IsNullOrWhiteSpace(query) ? DBNull.Value : query.Trim()));

        return ServiceResult<IReadOnlyCollection<ManageBookingResponse>>.Success(rows.Select(MapManageBooking).ToArray());
    }

    public async Task<ServiceResult<ManageBookingResponse>> CancelBooking(string storeId, string bookingId, BookingCancellationRequest request)
    {
        if (!request.CustomerContactConfirmed)
        {
            return ServiceResult<ManageBookingResponse>.ValidationError("CUSTOMER_CONTACT_NOT_CONFIRMED", "Customer contact must be confirmed before cancellation.");
        }

        if (!string.Equals(request.Reason, "STORE_CLOSURE", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<ManageBookingResponse>.Conflict("BOOKING_NOT_CANCELLABLE", "Only STORE_CLOSURE cancellation is currently supported.");
        }

        if (string.IsNullOrWhiteSpace(request.CancelledByUserId))
        {
            return ServiceResult<ManageBookingResponse>.ValidationError(
                "CANCELLATION_ACTOR_REQUIRED",
                "Cancellation actor user id is required.");
        }

        try
        {
            var rows = await _dbContext.ExecuteStoredProcedureAsync<StoreManagerBookingRow>(
                StoredProcedures.StoreManagerCancelBooking,
                new SqlParameter("@StoreId", storeId),
                new SqlParameter("@BookingId", bookingId),
                new SqlParameter("@CancellationReason", request.Reason.ToUpperInvariant()),
                new SqlParameter("@CustomerContactConfirmed", request.CustomerContactConfirmed),
                new SqlParameter("@CancelledByUserId", request.CancelledByUserId),
                new SqlParameter("@CancelledByDisplayName", (object?)request.CancelledByDisplayName ?? DBNull.Value));

            var row = rows.FirstOrDefault();
            return row is null
                ? ServiceResult<ManageBookingResponse>.NotFound("BOOKING_NOT_FOUND", $"Booking '{bookingId}' was not found for store '{storeId}'.")
                : ServiceResult<ManageBookingResponse>.Success(MapManageBooking(row));
        }
        catch (SqlException ex) when (ex.Number == 51031)
        {
            return ServiceResult<ManageBookingResponse>.NotFound("BOOKING_NOT_FOUND", $"Booking '{bookingId}' was not found for store '{storeId}'.");
        }
        catch (SqlException ex) when (ex.Number == 51032)
        {
            return ServiceResult<ManageBookingResponse>.Conflict("BOOKING_NOT_CANCELLABLE", ex.Message);
        }
        catch (SqlException ex) when (ex.Number == 51033)
        {
            return ServiceResult<ManageBookingResponse>.ValidationError("CUSTOMER_CONTACT_NOT_CONFIRMED", ex.Message);
        }
    }

    private static ManageBookingResponse MapManageBooking(StoreManagerBookingRow x) => new()
    {
        Id = ParseString(x.BookingId),
        StoreId = ParseString(x.StoreId),
        StoreSlotId = x.StoreSlotId.ToString(System.Globalization.CultureInfo.InvariantCulture),
        CustomerRef = x.CustomerRef ?? string.Empty,
        Status = x.Status ?? string.Empty,
        WindowStartAt = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(x.StartsAtUtc),
        WindowEndAt = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(x.EndsAtUtc),
        WindowCapacity = x.Capacity,
        CreatedAt = x.CreatedUtc,
        UpdatedAt = x.UpdatedUtc
    };

    private static bool IsDeleteClosureConflict(SqlException ex) => ex.Number is 547 or 50000;

    private async Task<StoreClosureResponse?> TryRecoverCreatedClosure(string storeId, StoreClosureRequest request)
    {
        var closuresResult = await GetClosures(storeId, request.ClosureDate, request.ClosureDate);
        if (!closuresResult.IsSuccess || closuresResult.Value is null)
        {
            return null;
        }

        return closuresResult.Value
            .Where(x => string.Equals(x.ClosureType, request.ClosureType, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(x.Reason ?? string.Empty, request.Reason ?? string.Empty, StringComparison.Ordinal))
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();
    }

    private static string ParseString(string? raw) => raw?.Trim() ?? string.Empty;
}
