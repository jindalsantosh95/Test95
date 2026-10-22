using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal
{
    public class Passenger
    {
        public string PassengerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<Meal> Meals { get; set; }
    }
}
