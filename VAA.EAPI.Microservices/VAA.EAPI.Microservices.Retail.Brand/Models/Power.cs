namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Power : AmenityBase
    {
        public string Chance { get; set; }
        public string Cost { get; set; }
        public string Distribution { get; set; }
        public string Exists { get; set; }
        public string MultipleAtSeat { get; set; }
        public string PowerOutlet { get; set; }
        public string UsbPort { get; set; }

        public static explicit operator Power(RouteHappy.Power source)
        {
            if (source == null) return null;

            return new()
            {
                DisplayText = source.DisplayText,
                Quality = source.Quality.ToString(),
                Type = source.Type.ToString(),
                Exists = source.Exists.ToString(),
                Cost = source.Cost.ToString(),
                Distribution = source.Distribution.ToString(),
                MultipleAtSeat = source.MultipleAtSeat.ToString(),
                Chance = source.Chance.ToString(),
                PowerOutlet = source.PowerOutlet.ToString(),
                UsbPort = source.UsbPort.ToString()
            };
        }
    }
}