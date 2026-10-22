using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class Affiliation
    {
        public AffiliationCodeType Code { get; set; }
        public string Owner { get; set; }

        public static explicit operator Affiliation(DeltaPaxInfo.Affiliation source)
        {
            if (source is null) return null;

            return new Affiliation()
            {
                Code = (AffiliationCodeType)source.Code,
                Owner = source.Owner
            };
        }
    }
}