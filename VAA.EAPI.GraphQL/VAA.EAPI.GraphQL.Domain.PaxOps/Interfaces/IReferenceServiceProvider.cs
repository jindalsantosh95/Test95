namespace VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces
{
    public interface IReferenceServiceProvider
    {
        Task<IEnumerable<Models.ReferenceData.Gender>> GetGenderDetails();
        Task<IEnumerable<Models.ReferenceData.StatesOfUSA>> GetStatesOfUSA();
        Task<IEnumerable<Models.ReferenceData.PassportCountry>> GetPassportCountries();
        Task<IEnumerable<Models.ReferenceData.Nationality>> GetNationalityDetails();
        Task<IEnumerable<Models.ReferenceData.CountryProfile>> GetCountryProfile();
        Task<IEnumerable<Models.ReferenceData.CurrencyNumberReferenceData>> GetAir4CurrencyNumberAsync();
        Task<IEnumerable<Models.ReferenceData.AirportDetails>> RetrieveAirportDataAsync();
    }
}
