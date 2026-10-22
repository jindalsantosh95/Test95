namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public class SlotRuleReplaceRequest
{
    public IEnumerable<SlotRuleRequest> Rules { get; set; } = Enumerable.Empty<SlotRuleRequest>();
}
