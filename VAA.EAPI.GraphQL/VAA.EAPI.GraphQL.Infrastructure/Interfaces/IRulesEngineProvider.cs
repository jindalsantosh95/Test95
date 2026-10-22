using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IRulesEngineProvider<T>
    {
        Task<T> InRulePointsCalculator(CustomerCustMS cutomerResponse, VSRetroFlownDetail pdmResponse);
        Task<T> InRulePointsCalculatorForSkyFlights(CustomerCustMS cutomerResponse, SkyClaimResponse skyResponse, AirportDistanceAndCountries airportsDistanceAndCountry, string cabinClass);
    }
}
