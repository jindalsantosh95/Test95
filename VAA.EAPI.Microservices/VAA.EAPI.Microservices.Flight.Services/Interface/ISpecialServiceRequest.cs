using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Flight.Services.Models.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Interface
{
    public interface ISpecialServiceRequest
    {
        Task<Result<Response<PassengerSSRResponse>>> GetSSRDetailsAsync(string tripBookingId);
        Task<Result<Response<PassengerSSRResponse>>> ManageSSRDetailsAsync(SSRUpsertRequest ssrUpsertRequest, string tripReservationID);
    }
}
