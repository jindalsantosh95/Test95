using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface ILOPAServiceProvider
    {
        Task<IEnumerable<SeatMap>> RetrieveAircraftDetailsAsync(string fleetTypeCode, string equipmentCode);
    }
}