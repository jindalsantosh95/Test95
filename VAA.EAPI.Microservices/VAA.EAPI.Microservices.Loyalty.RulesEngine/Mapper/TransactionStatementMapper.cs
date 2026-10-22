using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper
{
    public class TransactionStatementMapper
    {
        public static string MapStatement(RuleActionsResponse actionRequest)
        {
            if (!string.IsNullOrEmpty(actionRequest.ProductName))
            {
                switch (actionRequest.ProductName)
                {
                    case Constants.Application.BirthdayBonusProductName:
                        return Constants.Application.BirthdayStatement;
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
