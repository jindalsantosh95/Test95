namespace VAA.EAPI.Infrastructure.PSS.Entities.SeatAvailability
{
    public class SeatColumn
    {
        public bool Available { get; set; }
        public bool Aisle { get; set; }
        public bool Reserved { get; set; }
        public string Pos { get; set; }
        public string BrandId { get; set; }
        public string SeatProductId { get; set; }
        public int index { get; set; }
        public string OccupationCode { get; set; }
        public string SeatId { get; set; }
        public string CharacteristicCodes { get; set; }
    }
}
