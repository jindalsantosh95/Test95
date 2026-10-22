namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Beverage : AmenityBase
    {
        public string AlcoholicCost { get; set; }
        public string Exists { get; set; }
        public string NonAlcoholicCost { get; set; }

        public static explicit operator Beverage(RouteHappy.Beverage source)
        {
            if (source == null) return null;

            return new()
            {
                DisplayText = source.DisplayText,
                Quality = source.Quality.ToString(),
                Type = source.Type.ToString(),
                AlcoholicCost = source.AlcoholicCost.ToString(),
                Exists = source.Exists.ToString(),
                NonAlcoholicCost = source.NonalcoholicCost.ToString()
            };
        }
    }
}