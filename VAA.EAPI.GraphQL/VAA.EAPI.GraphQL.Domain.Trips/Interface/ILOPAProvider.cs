using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface ILOPAProvider
    {    
       Task<IEnumerable<SeatMap>> RetrieveAircraftDetailsAsync(string fleetTypeCode, string equipmentCode);
    }
}