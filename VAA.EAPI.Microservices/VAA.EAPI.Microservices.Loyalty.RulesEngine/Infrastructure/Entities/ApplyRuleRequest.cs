namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities
{
    public class RepositoryRuleAppRevisionSpec
    {
        public string RuleApplicationName { get; set; }
    }

    public class RuleApp
    {
        public string Password { get; set; }
        public RepositoryRuleAppRevisionSpec RepositoryRuleAppRevisionSpec { get; set; }
        public string RepositoryServiceUri { get; set; }
        public string UserName { get; set; }
    }

    public class ApplyRuleRequest
    {
        public RuleApp RuleApp { get; set; }
        public string EntityState { get; set; }
        public string EntityName { get; set; }
    }
}
