namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class StoreInformation
{
    public StoreContactDetails? Details { get; set; }
    public StoreOpeningHours? OpeningHours { get; set; }
    public StoreServiceList? Services { get; set; }
}
