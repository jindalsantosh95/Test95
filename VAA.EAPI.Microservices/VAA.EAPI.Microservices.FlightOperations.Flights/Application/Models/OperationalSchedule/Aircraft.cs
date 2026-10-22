using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.OperationalSchedule
{
    public class Aircraft
    {
        public string Registration { get; set; }

        [JsonProperty("Model_Code")]
        public string ModelCode { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }

    }
}
