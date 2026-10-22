using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class HealthStatus
    {
        public string Status { get; set; }

        public static explicit operator HealthStatus(DeltaPaxInfo.HealthStatus source)
        {
            if (source is null) return null;

            return new HealthStatus()
            {
                Status = source.Status
            };
        }
    }
}