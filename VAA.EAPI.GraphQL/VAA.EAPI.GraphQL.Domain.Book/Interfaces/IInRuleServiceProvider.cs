using VAA.EAPI.GraphQL.Domain.Book.Models.InRuleEngine;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IInRuleServiceProvider
    {
        Task<PointsEarnCalculatorResponse> AirlinePointsEarnCalculator(PointsEarnCalculatorRequest request);
    }
}
