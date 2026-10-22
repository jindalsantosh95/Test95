using VAA.EAPI.GraphQL.Domain.Account.Models;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IFlightQueryProvider
    {
        Task<FlightDetailsResponse> GetFlightsQueryData(string memberNumber, int page, int pageSize);
    }
}