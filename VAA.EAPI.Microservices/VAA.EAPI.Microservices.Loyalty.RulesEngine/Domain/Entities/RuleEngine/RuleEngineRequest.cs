using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class RuleEngineRequest
    {
        public string RuleApplicationName { get; set; }
        public string RuleEntityName { get; set; }
        public object RequestEntity { get; set; }
    }
    public class EntityState
    {
        public string CorrelationId { get; set; }
        public IEnumerable<TierRuleSetEntityRequest> Customers { get; set; }
        public string AirlineCodeOperating { get; set; }
        public string AirlineCodeMarketing { get; set; }
        public string FlightNumberOperating { get; set; }
        public string FlightNumberMarketing { get; set; }
        public string FlightDate { get; set; }
        public string FareBasisCode { get; set; } = string.Empty;
        public string FlownClass { get; set; } = string.Empty;
        public string SalesLocation { get; set; } = string.Empty;
        public string AgentIataNumber { get; set; } = string.Empty;
        public string FareBrand { get; set; } = string.Empty;
    }
}