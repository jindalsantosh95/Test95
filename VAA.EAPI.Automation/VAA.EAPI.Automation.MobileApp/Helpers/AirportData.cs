namespace VAA.EAPI.Automation.MobileApp.Helpers
{
    public class Airport
    {
        public string airportCode { get; set; } = string.Empty;
        public string airportName { get; set; } = string.Empty;
    }

    public class AirportList
    {
        public List<Airport> listOfCities { get; set; } = new List<Airport>();
    }
}
