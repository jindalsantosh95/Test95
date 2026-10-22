namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat
{
    public class Column
    {
        public string Type { get; set; }
        public string SeatNumber { get; set; }
        public string Availability { get; set; }
        public IEnumerable<string> Attributes { get; set; }
        public IEnumerable<Offer> Offers { get; set; }
        public bool? OverWing { get; set; }
        public IEnumerable<MarketingInfo> MarketingInfo { get; set; }
        public bool? OffSet { get; set; }
        public bool Missing { get; set; }
        public string Pos { get; set; }
        public bool EconomyComfort { get; set; }
        public bool ExitOverride { get; set; }
        public bool OverrideSeatType { get; set; }
        public string OverrideSeatTypeTo { get; set; }
    }
}