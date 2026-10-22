using VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class PassengerDetailsErrorResponse
    {
        public IEnumerable<Error> Errors { get; set; }
    }
}
