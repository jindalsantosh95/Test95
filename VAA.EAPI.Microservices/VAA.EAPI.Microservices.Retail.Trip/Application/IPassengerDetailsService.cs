using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public interface IPassengerDetailsService
    {
        Task<Result<Response<PassengerDetails>>> GetPassengerDetails(string tripBookingId);
        Task<Result<Response<PassengerDetails>>> UpatePassengerDetails(PassengerDetails passengerDetails, string tripBookingId);
    }
}
