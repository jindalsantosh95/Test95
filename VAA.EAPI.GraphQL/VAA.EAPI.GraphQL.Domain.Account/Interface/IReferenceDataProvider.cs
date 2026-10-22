using VAA.EAPI.GraphQL.Domain.Account.Models.Reference;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IReferenceDataProvider
    {
        Task<IEnumerable<CountryRefData>> RetrieveCountriesAsync();
    }
}
