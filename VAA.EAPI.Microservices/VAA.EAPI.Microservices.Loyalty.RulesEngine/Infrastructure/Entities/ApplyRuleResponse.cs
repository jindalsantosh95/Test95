using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities
{
    public class ActiveNotification
    {
        public object ElementId { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
    }

    public class ApplyRuleResponse
    {
        public List<ActiveNotification> ActiveNotifications { get; set; }
        public List<object> ActiveValidations { get; set; }
        public string EntityState { get; set; }
        public bool HasRuntimeErrors { get; set; }
        public object Overrides { get; set; }
        public object Parameters { get; set; }
        public string RequestId { get; set; }
        public object RuleExecutionLog { get; set; }
        public object RuleSessionState { get; set; }
        public string SessionId { get; set; }
    }
    public class InRuleResponse
    {
        public string RuleApplicationName { get; set; }
        public string RuleEntityName { get; set; }
        public List<ActiveNotification> ActiveNotifications { get; set; }
        public List<object> ActiveValidations { get; set; }
        public object ResponseEntity { get; set; }
        public bool HasRuntimeErrors { get; set; }
        public string SessionId { get; set; }
    }
}

