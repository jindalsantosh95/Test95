using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IRefServiceProvider
    {
        Task<IEnumerable<AirlineRefData>> RetrieveAirlineDataAsync();
        Task<IEnumerable<AirportRefData>> RetrieveAirportDataAsync();
        Task<IEnumerable<CabinRefData>> RetrieveCabinDataAsync();
        Task<IEnumerable<CurrencyNumberRefData>> RetrieveAir4CurrencyNumberAsync();
        Task<IEnumerable<AircraftRefData>> RetrieveAircraftDataAsync();
        Task<IEnumerable<CountryRefData>> RetrieveCountriesAsync();
        Task<IEnumerable<CabinClassRefDetails>> RetrieveCabinClassAsync();
        Task<IEnumerable<BlackListedEmails>> RetrieveBlackListedEmailsAsync();
    }
}