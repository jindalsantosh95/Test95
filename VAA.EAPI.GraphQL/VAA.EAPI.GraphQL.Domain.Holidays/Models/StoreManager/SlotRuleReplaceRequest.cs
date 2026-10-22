namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class SlotRuleReplaceRequest
{
    public IEnumerable<SlotRuleRequest> Rules { get; set; } = Enumerable.Empty<SlotRuleRequest>();
}

