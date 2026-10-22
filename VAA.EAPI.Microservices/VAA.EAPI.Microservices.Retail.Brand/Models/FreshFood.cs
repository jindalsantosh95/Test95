namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class FreshFood : AmenityBase
    {
        public string Cost { get; set; }
        public string Exists { get; set; }

        public static explicit operator FreshFood(RouteHappy.FreshFood source)
        {
            if (source == null) return null;

            return new()
            {
                DisplayText = source.DisplayText,
                Quality = source.Quality.ToString(),
                Type = source.Type.ToString(),
                Cost = source.Cost.ToString(),
                Exists = source.Exists.ToString(),
            };
        }
    }
}