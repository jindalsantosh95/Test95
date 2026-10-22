using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class ExploreStoreMasterDBHelper
    {
        private static readonly string connectionString = Vault.Default.GetSecretValue("Explore-HolidaysDbConnectionString");

        public static StoreMasterRecord? GetStoreById(string storeId)
        {
            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT * FROM dbo.StoreMaster WHERE StoreId = @StoreId", connection);

            command.Parameters.Add("@StoreId", System.Data.SqlDbType.NVarChar, 20).Value = storeId;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            StoreMasterRecord store = new()
            {
                StoreId = reader["StoreId"]?.ToString() ?? string.Empty,
                Name = reader["Name"]?.ToString() ?? string.Empty,
                Label = reader["Label"]?.ToString() ?? string.Empty,
                Phone = reader["Phone"]?.ToString() ?? string.Empty,
                EmailAddress = reader["EmailAddress"]?.ToString() ?? string.Empty,
                AddressLine1 = reader["AddressLine1"]?.ToString() ?? string.Empty,
                AddressLine2 = reader["AddressLine2"]?.ToString() ?? string.Empty,
                City = reader["City"]?.ToString() ?? string.Empty,
                County = reader["County"]?.ToString() ?? string.Empty,
                CountryCode = reader["CountryCode"]?.ToString() ?? string.Empty,
                PostalCode = reader["PostalCode"]?.ToString() ?? string.Empty,
                StoreWorkHoursJson = reader["StoreWorkHoursJson"]?.ToString() ?? string.Empty,
                StoreSpecialHoursJson = reader["StoreSpecialHoursJson"]?.ToString() ?? string.Empty,
                StoreImageSrc = reader["StoreImageSrc"]?.ToString() ?? string.Empty,
                StoreImageAlt = reader["StoreImageAlt"]?.ToString() ?? string.Empty,
                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                MarkedAsClosed = reader["MarkedAsClosed"] != DBNull.Value && Convert.ToBoolean(reader["MarkedAsClosed"])
            };

            store.WeeklyHours = store.StoreWorkHoursJson.IsNullOrWhiteSpace() ? new List<DbWeeklyHour>() : JsonConvert.DeserializeObject<List<DbWeeklyHour>>(store.StoreWorkHoursJson) ?? new List<DbWeeklyHour>();

            DbSpecialHoursWrapper? specialHoursWrapper = store.StoreSpecialHoursJson.IsNullOrWhiteSpace() ? null : JsonConvert.DeserializeObject<DbSpecialHoursWrapper>(store.StoreSpecialHoursJson);

            store.SpecialHours = specialHoursWrapper?.specialHours ?? new List<DbSpecialHour>();

            string servicesJson = reader["StoreServicesJson"]?.ToString() ?? string.Empty;

            if (!servicesJson.IsNullOrWhiteSpace())
            {
                store.Services = JsonConvert.DeserializeObject<List<string>>(servicesJson) ?? new List<string>();

                store.HasKidsArea = store.Services.Contains("Kids area", StringComparer.OrdinalIgnoreCase);

                store.HasWelcomeDrinksAndSnacks = store.Services.Contains("Welcome drinks and snacks", StringComparer.OrdinalIgnoreCase);

                store.HasAircraftSeats = store.Services.Contains("Aircraft seats", StringComparer.OrdinalIgnoreCase);

                store.HasSecretCinema = store.Services.Contains("Secret cinema", StringComparer.OrdinalIgnoreCase);
            }
            return store;
        }

        public static List<string> GetAllActiveStoreIds()
        {
            List<string> storeIds = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT StoreId FROM dbo.StoreMaster WHERE IsActive = 1", connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string? storeId = reader["StoreId"]?.ToString();

                if (!storeId.IsNullOrWhiteSpace())
                {
                    storeIds.Add(storeId);
                }
            }

            return storeIds;
        }

        public static List<StoreManagerSlotRulesHelper> GetSlotRulesByStoreId(string storeId)
        {
            List<StoreManagerSlotRulesHelper> rules = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT * FROM dbo.StoreManagerSlotRules WHERE StoreId = @StoreId ORDER BY DayOfWeek, StartTime", connection);

            command.Parameters.Add("@StoreId", System.Data.SqlDbType.NVarChar, 20).Value = storeId;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                StoreManagerSlotRulesHelper rule = new()
                {
                    SlotRuleId = reader["SlotRuleId"]?.ToString() ?? string.Empty,
                    StoreId = reader["StoreId"]?.ToString() ?? string.Empty,
                    DayOfWeek = reader["DayOfWeek"] != DBNull.Value ? Convert.ToInt32(reader["DayOfWeek"]) : -1,
                    StartTime = reader["StartTime"]?.ToString() ?? string.Empty,
                    EndTime = reader["EndTime"]?.ToString() ?? string.Empty,
                    SlotDurationMinutes = reader["SlotDurationMinutes"] != DBNull.Value ? Convert.ToInt32(reader["SlotDurationMinutes"]) : 0,
                    SlotCapacity = reader["SlotCapacity"] != DBNull.Value ? Convert.ToInt32(reader["SlotCapacity"]) : 0,
                    CreatedUtc = reader["CreatedUtc"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedUtc"]) : DateTime.MinValue,
                    UpdatedUtc = reader["UpdatedUtc"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedUtc"]) : DateTime.MinValue,
                    DayName = reader["DayName"]?.ToString() ?? string.Empty,
                    IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                };

                rules.Add(rule);
            }

            return rules;
        }

        public static bool HasActiveStoreClosure(string storeId, DateOnly date)
        {
            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.StoreManagerClosures WHERE StoreId = @StoreId AND ClosureDate = @ClosureDate AND IsActive = 1) THEN 1 ELSE 0 END", connection);

            command.Parameters.Add("@StoreId", System.Data.SqlDbType.NVarChar, 20).Value = storeId;

            command.Parameters.Add("@ClosureDate", System.Data.SqlDbType.Date).Value = date.ToDateTime(TimeOnly.MinValue);

            connection.Open();

            return Convert.ToBoolean(command.ExecuteScalar());
        }

        public static bool HasActiveStoreSlotsForDay(string storeId, string dayCode)
        {
            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.StoreSlot WHERE StoreId = @StoreId AND DayCode = @DayCode AND IsActive = 1) THEN 1 ELSE 0 END", connection);

            command.Parameters.Add("@StoreId", System.Data.SqlDbType.NVarChar, 20).Value = storeId;

            command.Parameters.Add("@DayCode", System.Data.SqlDbType.NVarChar, 3).Value = dayCode;

            connection.Open();

            return Convert.ToBoolean(command.ExecuteScalar());
        }

        public static StoreAppointmentRecord? GetAppointmentById(string appointmentId)
        {
            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT Id, AppointmentType, AppointmentReason, BookingReference, [Date], SlotStart, SlotEnd, LocationId, VisitPurpose, VisitPurposeDetails, AdditionalDetails, AllocationId, AppointmentStatus FROM dbo.StoreAppointment WHERE Id = @AppointmentId", connection);

            command.Parameters.Add("@AppointmentId", System.Data.SqlDbType.NVarChar, 20).Value = appointmentId;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new StoreAppointmentRecord
            {
                Id = reader["Id"]?.ToString() ?? string.Empty,
                AppointmentType = reader["AppointmentType"]?.ToString() ?? string.Empty,
                AppointmentReason = reader["AppointmentReason"]?.ToString() ?? string.Empty,
                BookingReference = reader["BookingReference"]?.ToString() ?? string.Empty,
                Date = DateOnly.FromDateTime(Convert.ToDateTime(reader["Date"])),
                SlotStart = TimeSpan.Parse(reader["SlotStart"]?.ToString() ?? "00:00:00"),
                SlotEnd = TimeSpan.Parse(reader["SlotEnd"]?.ToString() ?? "00:00:00"),
                LocationId = reader["LocationId"]?.ToString() ?? string.Empty,
                VisitPurpose = reader["VisitPurpose"]?.ToString() ?? string.Empty,
                VisitPurposeDetails = reader["VisitPurposeDetails"]?.ToString() ?? string.Empty,
                AdditionalDetails = reader["AdditionalDetails"]?.ToString() ?? string.Empty,
                AllocationId = reader["AllocationId"] == DBNull.Value ? null : reader["AllocationId"]?.ToString(),
                AppointmentStatus = reader["AppointmentStatus"]?.ToString() ?? string.Empty
            };
        }

        public static StoreSlotBookingState? GetSlotBookingState(string storeId, DateOnly date, DateTimeOffset slotStart, DateTimeOffset slotEnd)
        {
            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT TOP (1) slot.Capacity, CAST(CASE WHEN MAX(CASE WHEN (appointment.VisitPurpose = N'Store Manager' AND appointment.VisitPurposeDetails = N'BLOCKED') OR appointment.AppointmentReason = N'STORE_MANAGER_BLOCKED' THEN 1 ELSE 0 END) = 1 THEN 1 ELSE 0 END AS bit) AS HasBlockedAppointment, SUM(CASE WHEN (appointment.VisitPurpose = N'Store Manager' AND appointment.VisitPurposeDetails = N'BLOCKED') OR appointment.AppointmentReason = N'STORE_MANAGER_BLOCKED' THEN 0 WHEN appointment.AppointmentStatus = N'BOOKED' THEN 1 ELSE 0 END) AS BookedCount FROM dbo.StoreSlot slot LEFT JOIN dbo.StoreAppointment appointment ON appointment.LocationId = slot.StoreId AND appointment.[Date] = @Date AND appointment.SlotStart < slot.SlotEndTime AND appointment.SlotEnd > slot.SlotStartTime WHERE slot.StoreId = @StoreId AND slot.DayCode = LEFT(DATENAME(WEEKDAY, @Date), 3) AND slot.SlotStartTime = @SlotStart AND slot.SlotEndTime = @SlotEnd AND slot.IsActive = 1 GROUP BY slot.Capacity", connection);

            command.Parameters.Add("@StoreId", System.Data.SqlDbType.NVarChar, 20).Value = storeId;

            command.Parameters.Add("@Date", System.Data.SqlDbType.Date).Value = date.ToDateTime(TimeOnly.MinValue);

            command.Parameters.Add("@SlotStart", System.Data.SqlDbType.Time).Value = slotStart.TimeOfDay;

            command.Parameters.Add("@SlotEnd", System.Data.SqlDbType.Time).Value = slotEnd.TimeOfDay;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new StoreSlotBookingState
            {
                Capacity = Convert.ToInt32(reader["Capacity"]),
                BookedCount = Convert.ToInt32(reader["BookedCount"]),
                HasBlockedAppointment = Convert.ToBoolean(reader["HasBlockedAppointment"])
            };
        }

        public static List<StoreManagerClosure> GetClosuresByStoreId(string storeId, string from, string to)
        {
            List<StoreManagerClosure> closures = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT ClosureId, StoreId, ClosureDate, ClosureType, Note, IsActive, CreatedUtc, UpdatedUtc FROM dbo.StoreManagerClosures WHERE StoreId = @StoreId AND IsActive = 1 AND ClosureDate BETWEEN @FromDate AND @ToDate ORDER BY ClosureDate", connection);

            command.Parameters.Add("@StoreId", System.Data.SqlDbType.NVarChar, 20).Value = storeId;

            command.Parameters.Add("@FromDate", System.Data.SqlDbType.Date).Value = DateTime.Parse(from).Date;

            command.Parameters.Add("@ToDate", System.Data.SqlDbType.Date).Value = DateTime.Parse(to).Date;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                StoreManagerClosure closure = new()
                {
                    ClosureId = reader["ClosureId"]?.ToString() ?? string.Empty,
                    StoreId = reader["StoreId"]?.ToString() ?? string.Empty,
                    ClosureDate = Convert.ToDateTime(reader["ClosureDate"]).ToString("yyyy-MM-dd"),
                    ClosureType = reader["ClosureType"]?.ToString() ?? string.Empty,
                    Note = reader["Note"]?.ToString() ?? string.Empty,
                    IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                    CreatedUtc = reader["CreatedUtc"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedUtc"]) : DateTime.MinValue,
                    UpdatedUtc = reader["UpdatedUtc"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedUtc"]) : DateTime.MinValue
                };

                closures.Add(closure);
            }

            return closures;
        }

        public static List<StoreManagerSlotRulesHelper> GetActiveSlotRulesByStoreId(string storeId)
        {
            List<StoreManagerSlotRulesHelper> rules = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT SlotRuleId, StoreId, DayOfWeek, StartTime, EndTime, SlotDurationMinutes, SlotCapacity, IsActive, CreatedUtc, UpdatedUtc, DayName FROM dbo.StoreManagerSlotRules WHERE StoreId = @StoreId AND IsActive = 1 ORDER BY DayOfWeek, StartTime", connection);

            command.Parameters.Add("@StoreId", System.Data.SqlDbType.NVarChar, 20).Value = storeId;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                rules.Add(new StoreManagerSlotRulesHelper
                {
                    SlotRuleId = reader["SlotRuleId"]?.ToString() ?? string.Empty,
                    StoreId = reader["StoreId"]?.ToString() ?? string.Empty,
                    DayOfWeek = Convert.ToInt32(reader["DayOfWeek"]),
                    StartTime = reader["StartTime"]?.ToString() ?? string.Empty,
                    EndTime = reader["EndTime"]?.ToString() ?? string.Empty,
                    SlotDurationMinutes = Convert.ToInt32(reader["SlotDurationMinutes"]),
                    SlotCapacity = Convert.ToInt32(reader["SlotCapacity"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    CreatedUtc = reader["CreatedUtc"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedUtc"]) : DateTime.MinValue,
                    UpdatedUtc = reader["UpdatedUtc"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedUtc"]) : DateTime.MinValue,
                    DayName = reader["DayName"]?.ToString() ?? string.Empty
                });
            }

            return rules;
        }

        public static List<string> GetActiveStoresWithSlotRulesAndSlots()
        {
            List<string> storeIds = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT DISTINCT sm.StoreId FROM dbo.StoreMaster sm INNER JOIN dbo.StoreManagerSlotRules rules ON rules.StoreId = sm.StoreId AND rules.IsActive = 1 INNER JOIN dbo.StoreSlot slots ON slots.StoreId = sm.StoreId AND slots.IsActive = 1 WHERE sm.IsActive = 1 AND sm.MarkedAsClosed = 0 ORDER BY sm.StoreId", connection);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string storeId = reader["StoreId"]?.ToString() ?? string.Empty;

                if (!storeId.IsNullOrWhiteSpace())
                {
                    storeIds.Add(storeId);
                }
            }

            return storeIds;
        }

        public static List<StoreSlotRecord> GetStoreSlotsByStoreId(string storeId)
        {
            List<StoreSlotRecord> slots = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("SELECT StoreId, DayCode, SlotStartTime, SlotEndTime, Capacity, IsActive, Status FROM dbo.StoreSlot WHERE StoreId = @StoreId ORDER BY DayCode, SlotStartTime", connection);

            command.Parameters.Add("@StoreId", System.Data.SqlDbType.NVarChar, 20).Value = storeId;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                slots.Add(new StoreSlotRecord
                {
                    StoreId = reader["StoreId"]?.ToString() ?? string.Empty,
                    DayCode = reader["DayCode"]?.ToString() ?? string.Empty,
                    SlotStartTime = TimeSpan.Parse(reader["SlotStartTime"]?.ToString() ?? "00:00:00"),
                    SlotEndTime = TimeSpan.Parse(reader["SlotEndTime"]?.ToString() ?? "00:00:00"),
                    Capacity = Convert.ToInt32(reader["Capacity"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    Status = reader["Status"]?.ToString() ?? string.Empty
                });
            }

            return slots;
        }
    }
}
