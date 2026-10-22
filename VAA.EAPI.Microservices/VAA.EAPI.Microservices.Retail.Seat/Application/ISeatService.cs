using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Seats;

namespace VAA.EAPI.Microservices.Retail.Seat.Application
{
    public interface ISeatService
    {
        Task<Result<Response<string>>> DeleteSeatAsync(SeatDeletionRequest request);

        Task<Result<Response<SeatDeletionResponse>>> GetDeleteSeatResponseAsync(string id, string type);
    }
}
