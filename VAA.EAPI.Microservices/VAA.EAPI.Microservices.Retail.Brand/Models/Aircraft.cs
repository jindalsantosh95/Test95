namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Aircraft : AmenityBase
    {
        public string Cabin_Pressure { get; set; }
        public string Model { get; set; }
        public string Window_Size { get; set; }

        public static explicit operator Aircraft(RouteHappy.Aircraft source)
        {
            if (source == null) return null;

            return new()
            {
                DisplayText = source.DisplayText,
                Quality = source.Quality.ToString(),
                Type = source.Type.ToString(),
                Cabin_Pressure = source.CabinPressure.ToString(),
                Model = source.Model,
                Window_Size = source.WindowSize.ToString()
            };
        }
    }
}