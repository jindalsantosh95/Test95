using VAA.EAPI.GraphQL.Domain.Search.Models.InRuleEngine;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface IInRuleService
    {
        Task<PointsEarnCalculatorResponse> AirlinePointsEarnCalculator(PointsEarnCalculatorRequest inruleRequest);
    }
}
