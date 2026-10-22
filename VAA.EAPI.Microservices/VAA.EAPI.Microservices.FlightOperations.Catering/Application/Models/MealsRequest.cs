using System;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models
{
    public class MealsRequest
    {
        public DateOnly FlightDate { get; set; }
        public string Cabin { get; set; }
        public int FlightNo { get; set; }
        public string Carrier { get; set; }
    }
}
