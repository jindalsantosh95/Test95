using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class MoreInfo
    {
        public string Code { get; set; }
        public string Field { get; set; }
        public string Message { get; set; }

        public static explicit operator MoreInfo(DeltaPaxInfo.MoreInfo source)
        {
            if (source is null) return null;

            return new MoreInfo()
            {
                Code = source.Code,
                Field = source.Field,
                Message = source.Message
            };
        }
    }
}