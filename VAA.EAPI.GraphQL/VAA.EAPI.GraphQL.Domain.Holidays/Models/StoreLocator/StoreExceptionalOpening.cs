namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator
{
    public class StoreExceptionalOpening
    {
        public string Label { get; set; } = default!;
        public string Date { get; set; } = string.Empty;
        public string OpeningTime { get; set; } = string.Empty;
        public string ClosingTime { get; set; } = string.Empty;
        public bool IsClosed { get; set; }
    }
}
