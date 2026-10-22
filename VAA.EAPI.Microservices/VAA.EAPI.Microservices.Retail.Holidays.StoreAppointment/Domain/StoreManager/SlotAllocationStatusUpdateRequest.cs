using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public class SlotAllocationStatusUpdateRequest
{
    public SlotStatus? Status { get; set; }
    public string? Reason { get; set; }
}
