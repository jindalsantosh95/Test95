using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IInsuranceServiceProvider
    {
        Task<InsuranceQuoteResponse> InsuranceQuote(InsuranceQuoteRequest request);
    }
}
