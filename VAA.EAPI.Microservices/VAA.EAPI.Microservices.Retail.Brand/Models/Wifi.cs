namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Wifi : AmenityBase
    {
        public string Chance { get; set; }
        public string ConnectivityType { get; set; }
        public string Cost { get; set; }
        public string Coverage { get; set; }
        public string Exists { get; set; }
        public string Performance { get; set; }

        public static explicit operator Wifi(RouteHappy.Wifi source)
        {
            if (source == null) return null;

            return new()
            {
                DisplayText = source.DisplayText,
                Quality = source.Quality.ToString(),
                Type = source.Type.ToString(),
                Cost = source.Cost.ToString(),
                ConnectivityType = source.ConnectivityType.ToString(),
                Chance = source.Chance.ToString(),
                Coverage = source.Coverage.ToString(),
                Performance = source.Performance.ToString(),
            };
        }
    }
}