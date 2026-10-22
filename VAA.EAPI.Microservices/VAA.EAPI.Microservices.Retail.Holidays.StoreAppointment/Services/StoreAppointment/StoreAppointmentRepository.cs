using Microsoft.Data.SqlClient;
using System.Data;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;

public class StoreAppointmentRepository : IStoreAppointmentRepository
{
    private readonly StoreAppointmentDbContext _dbContext;

    public StoreAppointmentRepository(StoreAppointmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> GetLatestAppointmentReferenceIdAsync(string storeInitials)
    {
        var normalizedStoreInitials = (storeInitials ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedStoreInitials))
        {
            return null;
        }

        var references = await _dbContext.ExecuteStoredProcedureAsync<StoreAppointmentReferenceResult>(
            StoredProcedures.StoreAppointmentGetLatestReferenceId,
            new SqlParameter("@StoreInitials", normalizedStoreInitials));

        return references.FirstOrDefault()?.Id;
    }

    public async Task<StoreAppointmentSaveResult> AddAsync(Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);

        var isSuccessParameter = new SqlParameter("@IsSuccess", SqlDbType.Bit)
        {
            Direction = ParameterDirection.Output
        };

        var errorCodeParameter = new SqlParameter("@ErrorCode", SqlDbType.NVarChar, 50)
        {
            Direction = ParameterDirection.Output
        };

        var errorMessageParameter = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 300)
        {
            Direction = ParameterDirection.Output
        };

        await _dbContext.ExecuteStoredProcedureAsync(
            StoredProcedures.StoreAppointmentSave,
            new SqlParameter("@Id", appointment.Id),
            new SqlParameter("@AppointmentType", appointment.AppointmentType.ToString()),
            new SqlParameter("@AppointmentReason", appointment.AppointmentReason),
            new SqlParameter("@BookingReference", appointment.BookingReference),
            new SqlParameter("@Date", appointment.Date),
            new SqlParameter("@SlotStart", appointment.SlotStart),
            new SqlParameter("@SlotEnd", appointment.SlotEnd),
            new SqlParameter("@LocationId", appointment.LocationId),
            new SqlParameter("@VisitPurpose", appointment.VisitPurpose),
            new SqlParameter("@VisitPurposeDetails", appointment.VisitPurposeDetails),
            new SqlParameter("@AdditionalDetails", appointment.AdditionalDetails),
            isSuccessParameter,
            errorCodeParameter,
            errorMessageParameter);

        var hasExplicitResult = isSuccessParameter.Value is not null and not DBNull;
        var isSuccess = !hasExplicitResult || isSuccessParameter.Value is bool explicitResult && explicitResult;

        return new StoreAppointmentSaveResult
        {
            IsSuccess = isSuccess,
            ErrorCode = errorCodeParameter.Value is DBNull ? string.Empty : Convert.ToString(errorCodeParameter.Value) ?? string.Empty,
            ErrorMessage = errorMessageParameter.Value is DBNull ? string.Empty : Convert.ToString(errorMessageParameter.Value) ?? string.Empty
        };
    }
}
