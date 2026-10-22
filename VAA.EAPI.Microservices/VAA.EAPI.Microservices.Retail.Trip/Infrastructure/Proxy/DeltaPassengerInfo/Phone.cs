
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class Phone
    {
        public string SequenceNum { get; set; }
        public string DeviceType { get; set; }
        public string CountryDialingCode { get; set; }
        public string Number { get; set; }

        public static explicit operator Phone(Domain.PassengerDetails.Phone source)
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