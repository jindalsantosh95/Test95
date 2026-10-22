using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Services.SeatAvailability
{
    public interface IAir4SeatV8
    {
        Task<RetrievePassengerMapResponseType> RetrievePassengerMap(RetrievePassengerMapRequestType retrievePassengerMapRequestType);
        Task<AddResponseType> AddSeat(AddRequestType addRequestType);
        Task<DeleteResponseType> DeleteSeat(DeleteRequestType delRequestType);
    }
}


