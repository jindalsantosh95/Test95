using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IReferenceServiceProvider
    {
        Task<IEnumerable<CabinRefData>> RetrieveCabinDataAsync();
        Task<IEnumerable<SSRMealRefData>> RetrieveSSRMealDataAsync();
        Task<IEnumerable<AirportRefData>> RetrieveAirportDataAsync();
        Task<IEnumerable<FQTProgramRefData>> RetrieveLoyaltyProgramsDataAsync();
        Task<IEnumerable<CurrencyNumberRefData>> RetrieveAir4CurrencyNumberAsync();
        Task<IEnumerable<AirlineRefData>> RetrieveAirlineDetails();
        Task<IEnumerable<PaymentMethod>> RetrievePaymentMethodDetails();

    }
}