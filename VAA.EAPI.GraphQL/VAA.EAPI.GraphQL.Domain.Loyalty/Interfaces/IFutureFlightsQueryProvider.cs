using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FutureFlights;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IFutureFlightsQueryProvider
    {
        Task<FlightResponse> Query(FlightRequest flightRequest);
    }
}
