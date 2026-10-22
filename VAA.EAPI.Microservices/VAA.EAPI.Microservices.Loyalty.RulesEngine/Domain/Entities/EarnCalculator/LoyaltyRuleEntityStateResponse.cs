using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator
{
    public class LoyaltyRuleEntityStateResponse
    {
        public string AirlineCodeOperating { get; set; }

        public string FlightNumberOperating { get; set; }

        public string FlightDate { get; set; }

        public IEnumerable<Customer> Customers { get; set; }

    }
}
