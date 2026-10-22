namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class Affiliation
    {
        public AffiliationCodeType Code { get; set; }
        public string Owner { get; set; }

        public static explicit operator Affiliation(Infrastructure.Proxy.DeltaMessageCentre.Affiliation source)
        {
            if (source == null)
                return null;

            return new Affiliation()
            {
                Code = (AffiliationCodeType)source.Code,
                Owner = source.Owner
            };
        }
    }
}
