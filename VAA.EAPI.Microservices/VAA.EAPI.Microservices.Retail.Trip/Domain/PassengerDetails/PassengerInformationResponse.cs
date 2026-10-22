using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class PassengerDetails
    {
        public PassengerDetailsRequest PassengerInformation { get; set; }

        public static explicit operator PassengerDetails(DeltaPaxInfo.PassengerInformationResponse source)
        {
            if (source is null) return null;

            return new PassengerDetails()
            {
                PassengerInformation = (PassengerDetailsRequest)source.PassengerInformation
            };
        }
    }
}