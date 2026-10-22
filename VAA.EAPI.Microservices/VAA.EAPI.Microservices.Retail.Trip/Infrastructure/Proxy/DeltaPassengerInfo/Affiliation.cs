
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class Affiliation
    {
        public AffiliationCodeType Code { get; set; }
        public string Owner { get; set; }

        public static explicit operator Affiliation(Domain.PassengerDetails.Affiliation source)
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