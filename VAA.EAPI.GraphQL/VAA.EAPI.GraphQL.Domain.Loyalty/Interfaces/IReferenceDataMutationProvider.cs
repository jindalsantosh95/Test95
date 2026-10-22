using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IReferenceDataMutationProvider
    {
        Task<string> GetSegmentsDistance(string originAirportCode, string destinationAirportCode);

        Task<CabinClassDetailsResponse> GetCabinClassDetails(CabinClassDetailsRequest cabinClassDetailsRequest);
        Task<IEnumerable<CountryDetails>> GetCountryDetails();
        Task<AirportDistanceAndCountries> GetSegmentDistanceAndCountry(string originAirportCode, string destinationAirportCode);
    }
}
