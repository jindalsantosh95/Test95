namespace VAA.EAPI.Microservices.Retail.Lopa.Domain.Lopa
{

    public class Column
    {
        public string Type { get; set; }
        public string SeatNumber { get; set; }
        public string Availability { get; set; }
        public List<string> Attributes { get; set; }
        public List<Offer> Offers { get; set; }
        public bool OverWing { get; set; }
        public bool OffSet { get; set; }
        public bool Missing { get; set; }
        public string Pos { get; set; }
        public bool OverrideSeatType { get; set; }
        public string OverrideSeatTypeTo { get; set; }

    }

}