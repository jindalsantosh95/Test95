using InfrastructureSaveAppointmentBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.AppointmentBooking.AppointmentBookingResponse;

using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.AppointmentBooking;

public class AppointmentBookingResponse
{
    public string AppointmentReferenceId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string Store { get; set; } = string.Empty;
    public StoreSlot SlotDetails { get; set; } = new();

    public static explicit operator AppointmentBookingResponse(InfrastructureSaveAppointmentBookingResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new AppointmentBookingResponse
        {
            AppointmentReferenceId = response.AppointmentReferenceId,
            Date = response.Date,
            Store = response.Store,
            SlotDetails = response.SlotDetails is null ? new StoreSlot() : (StoreSlot)response.SlotDetails
        };
    }
}
