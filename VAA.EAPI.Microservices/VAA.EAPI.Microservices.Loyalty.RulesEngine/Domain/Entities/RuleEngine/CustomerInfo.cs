using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class CustomerInfo
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Distance { get; set; }
        public string OperatingBookedClass { get; set; }
        public string MarketingBookedClass { get; set; }
        public string AirlineCodeMarketing {  get; set; }
        public string AirlineCodeOperating {  get; set; }
        public string FlownClass { get; set; }
        public string SalesLocation { get; set; }
        public string AgentIataNumber { get; set; }
        public CustomerContext CustomerContext { get; set; }
        public IEnumerable<ErrorRuleLog> InvalidRuleLog { get; set; }
    }
}
