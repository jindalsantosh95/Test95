using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class Phone
    {
        public string SequenceNum { get; set; }
        public string DeviceType { get; set; }
        public string CountryDialingCode { get; set; }
        public string Number { get; set; }

        public static explicit operator Phone(DeltaPaxInfo.Phone source)
        {
            if (source is null) return null;

            return new Phone()
            {
                SequenceNum = source.SequenceNum,
                DeviceType = source.DeviceType,
                CountryDialingCode = source.CountryDialingCode,
                Number = source.Number
            };
        }
    }
}