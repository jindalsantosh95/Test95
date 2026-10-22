using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using MSSchema = VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public interface IMessageCentreService
    {
        Task<Result<Response<MSSchema.MessageResponse>>> GetMessageDetails(string tripBookingID);
    }
}
