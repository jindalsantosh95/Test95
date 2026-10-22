namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class OpeningHoursEntryRequest
{
    public int DayOfWeek { get; set; }
    public string OpensAt { get; set; } = string.Empty;
    public string ClosesAt { get; set; } = string.Empty;
    public bool CloseAfterOpen { get; set; }
}

