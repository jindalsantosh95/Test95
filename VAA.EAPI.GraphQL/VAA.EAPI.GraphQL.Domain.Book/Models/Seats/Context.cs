namespace VAA.EAPI.GraphQL.Domain.Book.Models.Seats
{
    public class Context
    {
        public bool IncludePrice { get; set; }
        public string CurrencyCode { get; set; }
        public bool IncludeRichContent { get; set; }
        public string RecordLocator { get; set; }
        public string DeviceType { get; set; }
    }
}
