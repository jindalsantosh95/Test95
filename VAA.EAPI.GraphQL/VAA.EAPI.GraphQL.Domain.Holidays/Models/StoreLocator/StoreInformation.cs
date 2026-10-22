namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator
{
    public class StoreInformation
    {
        public StoreContactDetails Details { get; set; } = new();
        public StoreOpeningHours OpeningHours { get; set; } = new();
        public StoreServiceList Services { get; set; } = new();
    }
}
