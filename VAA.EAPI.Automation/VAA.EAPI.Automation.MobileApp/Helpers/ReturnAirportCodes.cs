using System.Text.Json;

namespace VAA.EAPI.Automation.MobileApp.Helpers
{
    public class ReturnAirportCodes
    {
        public List<string> ReturnAllAirportCodes(string jsonData)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            AirportList airportList = JsonSerializer.Deserialize<AirportList>(jsonData, options)
                ?? throw new InvalidOperationException("Airport data could not be deserialized.");

            List<string> airportCodes = new List<string>();

            foreach (Airport airport in airportList.listOfCities)
            {
                airportCodes.Add(airport.airportCode);
            }

            return airportCodes;
        }

        public List<string> ReturnRandomSpecifiedAmountOfAirportCodes(string jsonData, int amount)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            AirportList airportList = JsonSerializer.Deserialize<AirportList>(jsonData, options)
                ?? throw new InvalidOperationException("Airport data could not be deserialized.");

            RandomSelection randomSelection = new RandomSelection();

            List<string> airportCodes = new List<string>();

            foreach (Airport airport in airportList.listOfCities)
            {
                airportCodes.Add(airport.airportCode);
            }

            List<string> randomAirportCodes = randomSelection.GetRandomAirportCodes(airportCodes, amount);

            return randomAirportCodes;
        }

        public class RandomSelection
        {
            public List<string> GetRandomAirportCodes(List<string> airportCodes, int numberOfItems)
            {
                Random rand = new Random();

                // Select the specified number of random items from the list
                List<string> randomAirportCodes = airportCodes
                    .OrderBy(x => rand.Next()) // Shuffle the list using random values
                    .Take(numberOfItems)       // Take the first 'numberOfItems' after shuffling
                    .ToList();

                return randomAirportCodes;
            }
        }
    }
}
