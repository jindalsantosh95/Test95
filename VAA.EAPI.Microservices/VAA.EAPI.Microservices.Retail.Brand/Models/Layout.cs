namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Layout : AmenityBase
    {
        public string DirectAisleAccess { get; set; }
        public string RowLayout { get; set; }

        public static explicit operator Layout(RouteHappy.Layout source)
        {
            if (source == null) return null;

            return new()
            {
                DisplayText = source.DisplayText,
                Quality = source.Quality.ToString(),
                Type = source.Type.ToString(),
                RowLayout = source.RowLayout,
                DirectAisleAccess = source.DirectAisleAccess.ToString()

            };
        }
    }
}